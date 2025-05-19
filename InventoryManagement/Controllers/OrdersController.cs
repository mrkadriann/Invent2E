using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Controllers
{
    public class OrdersController : Controller
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(InventoryDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(string searchString, string status, string orderSource, string location,
            DateTime? orderDateFrom, DateTime? orderDateTo, decimal? minOrderValue, decimal? maxOrderValue)
        {
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSource = orderSource;
            ViewBag.CurrentLocation = location;
            ViewBag.CurrentDateFrom = orderDateFrom?.ToString("yyyy-MM-dd");
            ViewBag.CurrentDateTo = orderDateTo?.ToString("yyyy-MM-dd");
            ViewBag.CurrentMinValue = minOrderValue;
            ViewBag.CurrentMaxValue = maxOrderValue;

            ViewBag.Statuses = new[] { "New", "Processing", "Shipped", "Delivered", "Fulfilled", "Cancelled" };
            ViewBag.Locations = new[] { "Manila", "Quezon City", "Makati", "Pasig", "Taguig" };

            var filteredOrders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                filteredOrders = filteredOrders.Where(o =>
                    o.OrderID.ToString().Contains(searchString) ||
                    (o.CustomerID != null && o.CustomerID.ToString().Contains(searchString)) || // Added null check for CustomerID
                    o.ShippingAddress.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                filteredOrders = filteredOrders.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(orderSource))
            {
                filteredOrders = filteredOrders.Where(o => o.OrderSource == orderSource);
            }

            if (!string.IsNullOrEmpty(location))
            {
                filteredOrders = filteredOrders.Where(o => o.Location == location);
            }

            if (orderDateFrom.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= orderDateFrom.Value);
            }

            if (orderDateTo.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= orderDateTo.Value);
            }

            if (minOrderValue.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderTotal >= minOrderValue.Value);
            }

            if (maxOrderValue.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderTotal <= maxOrderValue.Value);
            }

            return View(filteredOrders.ToList());
        }

        public IActionResult Create()
        {
            // This view might not be used if RecordPayment is the primary creation path
            // If it is, ensure it has necessary ViewBag data or a ViewModel
            return View(new Order { OrderDate = DateTime.Now });
        }

        public async Task<IActionResult> RecordPayment()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Description)
                    .Where(p => p.Quantity != null && p.Quantity.Qty > 0)
                    .OrderBy(p => p.ItemName)
                    .ToListAsync();

                _logger.LogInformation("Found {count} active products for RecordPayment GET", products.Count);
                ViewBag.Products = products;

                return View(new Order
                {
                    OrderDate = DateTime.Now,
                    Status = "New",
                    IsPaid = false,
                    OrderSource = "Website", // Default value
                    Location = "Manila",   // Default value
                    ProgressPercentage = 0,
                    ShippingAddress = "",
                    PaymentMethod = "",
                    ReferenceNumber = "",
                    TrackingNumber = "",
                    TrackingProvider = ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products in RecordPayment GET: {message}", ex.Message);
                ViewBag.Products = new List<Product>(); // Ensure ViewBag.Products is not null
                // Return view with an empty order model or handle error appropriately
                return View(new Order { OrderDate = DateTime.Now, Status = "New" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(Order order, string OrderItemsJson)
        {
            _logger.LogInformation("Starting RecordPayment POST action.");
            _logger.LogInformation("Received OrderItemsJson: {OrderItemsJson}", OrderItemsJson);

            try
            {
                if (string.IsNullOrEmpty(OrderItemsJson))
                {
                    ModelState.AddModelError("OrderItemsJson", "Order items cannot be empty.");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {errors}",
                        string.Join("; ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));
                    await ReloadProductsForView();
                    return View(order);
                }

                List<OrderItem> orderItems;
                try
                {
                    // Optional: For handling potential JS casing issues (e.g. itemId vs ItemId)
                    // var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    // orderItems = JsonSerializer.Deserialize<List<OrderItem>>(OrderItemsJson, serializerOptions) ?? new List<OrderItem>();

                    orderItems = JsonSerializer.Deserialize<List<OrderItem>>(OrderItemsJson) ?? new List<OrderItem>();

                    // Log deserialized items for further diagnosis
                    if (orderItems.Any())
                    {
                        foreach (var oi in orderItems)
                        {
                            _logger.LogInformation("Deserialized OrderItem: ItemId={OrderItemId}, Quantity={OrderItemQuantity}", oi.ItemId, oi.Quantity);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("OrderItemsJson deserialized to an empty list, or was originally null/empty.");
                        // This check is now more robust and should be caught by client-side validation too.
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize OrderItemsJson: {JsonString}", OrderItemsJson);
                    ModelState.AddModelError("OrderItemsJson", "Invalid order items data format.");
                    await ReloadProductsForView();
                    return View(order);
                }

                if (!orderItems.Any()) // Check if list is empty after deserialization
                {
                    ModelState.AddModelError("", "Order must contain at least one item.");
                    await ReloadProductsForView();
                    return View(order);
                }

                order.OrderItems = new List<OrderItem>(); // Initialize if null
                decimal calculatedOrderTotal = 0;

                foreach (var item in orderItems)
                {
                    if (item.ItemId <= 0) // Explicit check for invalid ItemId
                    {
                        _logger.LogError("Invalid ItemId received: {ItemId}", item.ItemId);
                        ModelState.AddModelError("", $"Invalid Product ID {item.ItemId} received. Ensure a valid product is selected.");
                        await ReloadProductsForView();
                        return View(order);
                    }

                    var product = await _context.Products
                        .Include(p => p.Description)
                        .FirstOrDefaultAsync(p => p.ItemId == item.ItemId);

                    if (product == null)
                    {
                        _logger.LogError("Product not found for ItemId: {ItemId}", item.ItemId);
                        ModelState.AddModelError("", $"Product ID {item.ItemId} not found. It may no longer be available.");
                        await ReloadProductsForView();
                        return View(order);
                    }

                    if (product.Description == null)
                    {
                        _logger.LogError("Product description not found for ItemId: {ItemId}, cannot determine price.", item.ItemId);
                        ModelState.AddModelError("", $"Product ID {item.ItemId} has no pricing information.");
                        await ReloadProductsForView();
                        return View(order);
                    }

                    decimal retailPriceValue = product.Description.RetailPrice.Value; // Get the non-nullable decimal

                    var orderItem = new OrderItem // Create new OrderItem instance to add to context
                    {
                        ItemId = product.ItemId,
                        Quantity = item.Quantity,
                        UnitPrice = retailPriceValue,
                        TotalPrice = item.Quantity * retailPriceValue
                        // OrderId will be set by EF relationship fixup
                    };
                    order.OrderItems.Add(orderItem);
                    calculatedOrderTotal += orderItem.TotalPrice;
                }

                order.OrderTotal = calculatedOrderTotal;

                // If IsPaid is true, ensure PaymentMethod is provided
                if (order.IsPaid && string.IsNullOrEmpty(order.PaymentMethod))
                {
                    ModelState.AddModelError(nameof(Order.PaymentMethod), "Payment method is required if order is paid.");
                }
                // If PaymentMethod is Cash, ensure PaymentAmount is at least OrderTotal
                if (order.IsPaid && order.PaymentMethod == "Cash" && order.PaymentAmount < order.OrderTotal)
                {
                    ModelState.AddModelError(nameof(Order.PaymentAmount), "Amount paid must be equal to or greater than the order total for cash payments.");
                }
                // If PaymentMethod requires a reference number, ensure it's provided
                if (order.IsPaid && (order.PaymentMethod == "GCash" || order.PaymentMethod == "Maya" || order.PaymentMethod == "Bank") && string.IsNullOrEmpty(order.ReferenceNumber))
                {
                    ModelState.AddModelError(nameof(Order.ReferenceNumber), "Reference number is required for this payment method.");
                }

                if (!ModelState.IsValid) // Re-check ModelState after custom validations
                {
                    _logger.LogWarning("ModelState became invalid after custom server-side validations.");
                    await ReloadProductsForView();
                    return View(order);
                }


                order.ShippingAddress = BuildShippingAddress(Request.Form);
                order.CustomerName = Request.Form["CustomerName"].ToString(); // Assuming CustomerName is not part of Order model directly
                // order.CustomerID is set via hidden field, if needed.

                _context.Orders.Add(order);
                // _logger.LogInformation("Database context state (before SaveChanges): {state}", _context.ChangeTracker.DebugView.LongView); // Can be very verbose

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync completed with {result} changes", result);

                if (result > 0)
                {
                    _logger.LogInformation("Order saved successfully with ID: {orderId}", order.OrderID);
                    TempData["SuccessMessage"] = $"Order #{order.OrderID} saved successfully!";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning("No changes were saved to the database for order.");
                ModelState.AddModelError("", "No changes were saved to the database. Please review order details and try again.");
                await ReloadProductsForView();
                return View(order);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving order. InnerException: {InnerEx}", ex.InnerException?.Message);
                ModelState.AddModelError("", "A database error occurred: " + (ex.InnerException?.Message ?? ex.Message) + ". Please try again.");
                await ReloadProductsForView();
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error in RecordPayment POST action");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                await ReloadProductsForView();
                return View(order);
            }
        }

        // ... (Invoice, GetInvoice, GetOrderDetails, UpdateOrder, ArchiveOrder, ArchiveHistory, RestoreOrder, DeleteOrder methods remain the same)
        public async Task<IActionResult> Invoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.OrderID == id);

            if (invoice == null)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Description) // Ensure product description for price/name
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                    return NotFound();

                invoice = new Invoice
                {
                    OrderID = order.OrderID,
                    InvoiceDate = order.OrderDate, // Set invoice date from order date
                    //CustomerName = order.CustomerName, // Assuming CustomerName is on Order
                    PaymentMethod = order.PaymentMethod,
                    ShippingAddress = order.ShippingAddress,
                    Status = order.IsPaid ? "Paid" : "Unpaid", // Invoice status based on order payment
                    Subtotal = order.OrderItems.Sum(oi => oi.TotalPrice), // Calculate subtotal from items
                    // Add Tax, Shipping if applicable
                    TotalAmount = order.OrderTotal,
                    PaymentAmount = order.IsPaid ? order.PaymentAmount : 0, // Use PaymentAmount from Order if paid
                    PaymentDate = order.IsPaid ? (order.OrderDate) : (DateTime?)null, // Or a specific payment date field if exists
                    ReferenceNumber = order.ReferenceNumber
                };

                foreach (var item in order.OrderItems)
                {
                    invoice.Items.Add(new InvoiceItem
                    {
                        ProductID = item.ItemId,
                        ProductName = item.Product?.ItemName ?? "Unknown Product",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    _logger.LogWarning("GetInvoice: Order not found for ID {OrderId}", id);
                    return Json(new { success = false, message = "Order not found" });
                }

                var invoice = new
                {
                    invoiceNumber = $"INV-{order.OrderID:D6}",
                    customerName = order.CustomerName,
                    invoiceDate = order.OrderDate.ToString("yyyy-MM-dd"),
                    paymentMethod = order.PaymentMethod,
                    shippingAddress = order.ShippingAddress,
                    status = order.Status, // Or more specific invoice status if different
                    items = order.OrderItems.Select(oi => new
                    {
                        productName = oi.Product?.ItemName ?? "Unknown Product",
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        totalPrice = oi.TotalPrice
                    }).ToList(),
                    subtotal = order.OrderItems.Sum(oi => oi.TotalPrice),
                    // tax = 0, // Example: if you have tax
                    // shipping = 0, // Example: if you have shipping cost
                    totalAmount = order.OrderTotal, // This should be sum of subtotal, tax, shipping
                    paymentAmount = order.PaymentAmount, // Amount actually paid
                    amountDue = order.OrderTotal - order.PaymentAmount // If partial payments allowed
                };
                _logger.LogInformation("GetInvoice: Successfully retrieved invoice data for Order ID {OrderId}", id);
                return Json(new { success = true, invoice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice for order {OrderId}", id);
                return Json(new { success = false, message = "Error retrieving invoice details." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            try
            {
                var order = await _context.Orders
                    // .Include(o => o.OrderItems) // Not strictly needed if only returning top-level order details
                    //     .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    _logger.LogWarning("GetOrderDetails: Order not found for ID {OrderId}", id);
                    return Json(new { success = false, message = "Order not found" });
                }
                _logger.LogInformation("GetOrderDetails: Successfully retrieved details for Order ID {OrderId}", id);
                return Json(new
                {
                    success = true,
                    order = new
                    {
                        orderID = order.OrderID,
                        status = order.Status,
                        trackingProvider = order.TrackingProvider,
                        trackingNumber = order.TrackingNumber,
                        progressPercentage = order.ProgressPercentage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderDetails for Order ID {OrderId}", id);
                return Json(new { success = false, message = "An error occurred while fetching order details." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int id, [FromForm] string status, [FromForm] string trackingProvider,
            [FromForm] string trackingNumber, [FromForm] int progressPercentage)
        {
            _logger.LogInformation("UpdateOrder: Attempting to update Order ID {OrderId} with Status: {Status}, Provider: {Provider}, Number: {Number}, Progress: {Progress}",
                id, status, trackingProvider, trackingNumber, progressPercentage);
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("UpdateOrder: Order not found for ID {OrderId}", id);
                    return Json(new { success = false, message = "Order not found" });
                }

                order.Status = status;
                order.TrackingProvider = trackingProvider;
                order.TrackingNumber = trackingNumber;
                order.ProgressPercentage = progressPercentage;

                await _context.SaveChangesAsync();
                _logger.LogInformation("UpdateOrder: Successfully updated Order ID {OrderId}", id);
                return Json(new { success = true });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "UpdateOrder: Concurrency error for Order ID {OrderId}", id);
                return Json(new { success = false, message = "The order was modified by another user. Please reload and try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOrder: Error updating Order ID {OrderId}", id);
                return Json(new { success = false, message = "An error occurred while updating the order." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveOrder(int id, string reason = "Archived by user")
        {
            _logger.LogInformation("ArchiveOrder: Attempting to archive Order ID {OrderId} with reason: {Reason}", id, reason);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    _logger.LogWarning("ArchiveOrder: Order not found for ID {OrderId}", id);
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Order not found" });
                }

                if (order.IsArchived)
                {
                    _logger.LogWarning("ArchiveOrder: Order ID {OrderId} is already archived.", id);
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Order is already archived." });
                }

                var archivedOrder = new ArchivedOrder
                {
                    // Map all relevant fields from Order to ArchivedOrder
                    OriginalOrderID = order.OrderID,
                    CustomerID = order.CustomerID,
                    CustomerName = order.CustomerName,
                    OrderDate = order.OrderDate,
                    OrderTotal = order.OrderTotal,
                    ShippingAddress = order.ShippingAddress,
                    Status = order.Status, // Status at time of archival
                    PaymentMethod = order.PaymentMethod,
                    IsPaid = order.IsPaid,
                    PaymentAmount = order.PaymentAmount,
                    ReferenceNumber = order.ReferenceNumber,
                    OrderSource = order.OrderSource,
                    Location = order.Location, // Location at time of archival
                    TrackingProvider = order.TrackingProvider,
                    TrackingNumber = order.TrackingNumber,
                    ProgressPercentage = order.ProgressPercentage,
                    ArchivedDate = DateTime.Now, // Date of archival action
                    ArchiveReason = reason
                };

                _context.ArchivedOrders.Add(archivedOrder);
                await _context.SaveChangesAsync(); // Save to get ArchivedOrderID

                foreach (var item in order.OrderItems)
                {
                    _context.ArchivedOrderItems.Add(new ArchivedOrderItem
                    {
                        ArchivedOrderID = archivedOrder.ArchivedOrderID, // Link to the new ArchivedOrder
                        ProductID = item.ItemId, // Original Product ID
                        ProductName = item.Product?.ItemName ?? "Unknown Product",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }

                var archiveHistoryEntry = new ArchiveHistory
                {
                    OrderID = order.OrderID, // Original Order ID
                    ArchivedOrderID = archivedOrder.ArchivedOrderID, // FK to ArchivedOrder
                    ArchivedBy = User.Identity?.Name ?? "System",
                    ArchiveDate = archivedOrder.ArchivedDate, // Consistent archive date
                    ArchiveReason = reason,
                    PreviousStatus = order.Status,
                    PreviousLocation = order.Location // Assuming Location is a relevant field to track
                };
                _context.ArchiveHistories.Add(archiveHistoryEntry);

                // Mark original order as archived
                order.IsArchived = true;
                order.ArchivedDate = archivedOrder.ArchivedDate;
                order.ArchiveReason = reason;
                // Optionally, remove items from the original order if your design moves them entirely
                // _context.OrderItems.RemoveRange(order.OrderItems); // If items are moved, not copied

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("ArchiveOrder: Successfully archived Order ID {OrderId}, new ArchivedOrderID is {ArchivedOrderId}", id, archivedOrder.ArchivedOrderID);
                return Json(new { success = true, message = "Order archived successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error archiving order {OrderId}: {Message}", id, ex.Message);
                return Json(new { success = false, message = $"Error archiving order: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ArchiveHistory(
            string searchString,
            string archivedBy,
            DateTime? archiveDateFrom,
            DateTime? archiveDateTo,
            string previousStatus,
            string previousLocation)
        {
            _logger.LogInformation("Retrieving archive history with filters - Search: {Search}, ArchivedBy: {ArchivedBy}, DateFrom: {DateFrom}, DateTo: {DateTo}, Status: {Status}, Location: {Location}",
                searchString, archivedBy, archiveDateFrom, archiveDateTo, previousStatus, previousLocation);
            try
            {
                ViewBag.CurrentSearch = searchString;
                ViewBag.CurrentArchivedBy = archivedBy;
                ViewBag.CurrentDateFrom = archiveDateFrom?.ToString("yyyy-MM-dd");
                ViewBag.CurrentDateTo = archiveDateTo?.ToString("yyyy-MM-dd");
                ViewBag.CurrentStatus = previousStatus;
                ViewBag.CurrentLocation = previousLocation;

                ViewBag.ArchivedByList = await _context.ArchiveHistories
                    .Select(h => h.ArchivedBy)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToListAsync();

                // Consider predefining these or fetching from distinct values if dynamic
                ViewBag.Statuses = new[] { "New", "Processing", "Shipped", "Delivered", "Fulfilled", "Cancelled" };
                ViewBag.Locations = await _context.Orders.Select(o => o.Location).Distinct().OrderBy(l => l).ToListAsync(); // Dynamic locations


                var query = _context.ArchiveHistories
                    .Include(h => h.Order) // Original order (might be minimal after archive)
                    .Include(h => h.ArchivedOrder) // The full archived copy
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(h =>
                        h.OrderID.ToString().Contains(searchString) || // Search by original OrderID
                        (h.ArchivedOrder != null && h.ArchivedOrder.OriginalOrderID.ToString().Contains(searchString)) || // Search by OriginalOrderID in ArchivedOrder
                        (h.ArchivedOrder != null && h.ArchivedOrder.CustomerName.Contains(searchString)) || // Search by CustomerName in ArchivedOrder
                        h.ArchiveReason.Contains(searchString));
                }

                if (!string.IsNullOrEmpty(archivedBy))
                {
                    query = query.Where(h => h.ArchivedBy == archivedBy);
                }

                if (archiveDateFrom.HasValue)
                {
                    query = query.Where(h => h.ArchiveDate >= archiveDateFrom.Value);
                }

                if (archiveDateTo.HasValue)
                {
                    // Add 1 day to archiveDateTo to make it inclusive of the selected date
                    query = query.Where(h => h.ArchiveDate < archiveDateTo.Value.AddDays(1));
                }

                if (!string.IsNullOrEmpty(previousStatus))
                {
                    query = query.Where(h => h.PreviousStatus == previousStatus);
                }

                if (!string.IsNullOrEmpty(previousLocation))
                {
                    query = query.Where(h => h.PreviousLocation == previousLocation);
                }

                var archiveHistoryEntries = await query
                    .OrderByDescending(h => h.ArchiveDate)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} archive history entries.", archiveHistoryEntries.Count);
                return View(archiveHistoryEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving archive history: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Error retrieving archive history. Please try again.";
                return View(new List<ArchiveHistory>());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreOrder(int id) // id here is Original OrderID
        {
            _logger.LogInformation("RestoreOrder: Attempting to restore Order ID {OrderId}", id);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.FindAsync(id); // Find original order
                if (order == null)
                {
                    _logger.LogWarning("RestoreOrder: Original Order ID {OrderId} not found.", id);
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Original order not found." });
                }

                if (!order.IsArchived)
                {
                    _logger.LogWarning("RestoreOrder: Order ID {OrderId} is not archived.", id);
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Order is not archived or already restored." });
                }

                var archiveHistoryEntry = await _context.ArchiveHistories
                    .Include(ah => ah.ArchivedOrder)
                        .ThenInclude(ao => ao.ArchivedOrderItems)
                    .FirstOrDefaultAsync(h => h.OrderID == id && h.ArchivedOrder != null);

                if (archiveHistoryEntry == null || archiveHistoryEntry.ArchivedOrder == null)
                {
                    _logger.LogWarning("RestoreOrder: Archive history or archived order data not found for Order ID {OrderId}.", id);
                    // Fallback: just unarchive the order status if full data is missing
                    order.IsArchived = false;
                    order.ArchivedDate = null;
                    order.ArchiveReason = null;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Order status restored (archive details missing)." });
                }

                // Restore properties from ArchivedOrder to Order
                order.IsArchived = false;
                order.ArchivedDate = null;
                order.ArchiveReason = null;
                // order.Status = archiveHistoryEntry.PreviousStatus; // Restore to previous status
                // order.Location = archiveHistoryEntry.PreviousLocation; // Restore previous location

                // If order items were moved, restore them from ArchivedOrderItems
                // This part depends on your archiving strategy (copy vs move)
                // Assuming a "copy" strategy where original items might have been cleared or marked.
                // If they were removed, re-add them:
                var currentOrderItems = await _context.OrderItems.Where(oi => oi.OrderID == order.OrderID).ToListAsync();
                _context.OrderItems.RemoveRange(currentOrderItems); // Clear existing (potentially outdated) items first

                foreach (var archivedItem in archiveHistoryEntry.ArchivedOrder.ArchivedOrderItems)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderID = order.OrderID,
                        ItemId = archivedItem.ProductID, // ProductID from ArchivedOrderItem
                        Quantity = archivedItem.Quantity,
                        UnitPrice = archivedItem.UnitPrice,
                        TotalPrice = archivedItem.TotalPrice
                    });
                }

                // Remove the ArchivedOrder, ArchivedOrderItems, and ArchiveHistory
                _context.ArchivedOrderItems.RemoveRange(archiveHistoryEntry.ArchivedOrder.ArchivedOrderItems);
                _context.ArchivedOrders.Remove(archiveHistoryEntry.ArchivedOrder);
                _context.ArchiveHistories.Remove(archiveHistoryEntry);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("RestoreOrder: Successfully restored Order ID {OrderId}.", id);
                return Json(new { success = true, message = "Order restored successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error restoring order {OrderId}: {Message}", id, ex.Message);
                return Json(new { success = false, message = $"Error restoring order: {ex.Message}" });
            }
        }


        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(int id) // id is the OrderID of an *archived* order's history entry, or Original OrderID
        {
            _logger.LogInformation("DeleteOrder: Starting permanent deletion for original OrderID {OrderId}", id);
            // This method should delete an order that is in the archive history,
            // meaning it will delete ArchiveHistory, ArchivedOrder, and ArchivedOrderItems.
            // The original Order (if it still exists and is marked IsArchived=true) might also be deleted or handled.

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the archive history entry using the original OrderID
                var archiveHistory = await _context.ArchiveHistories
                    .Include(ah => ah.ArchivedOrder) // Eager load ArchivedOrder
                        .ThenInclude(ao => ao.ArchivedOrderItems) // Then Eager load its items
                    .FirstOrDefaultAsync(h => h.OrderID == id);

                if (archiveHistory == null)
                {
                    _logger.LogWarning("DeleteOrder: No archive history found for original OrderID {OrderId}. Checking if it's a non-archived order.", id);
                    // If no archive history, it might be a request to delete a non-archived order,
                    // or an orphaned archived order. The current logic in question seems to expect an archived one.
                    // For safety, let's assume we only delete from archive. If you want to delete active orders, that's different.

                    // Try deleting the order directly if it's not archived or archive history is missing
                    var orderToDelete = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderID == id);
                    if (orderToDelete != null)
                    {
                        _logger.LogInformation("DeleteOrder: Found order (ID: {OrderId}) without archive history. Proceeding with direct deletion.", id);
                        _context.OrderItems.RemoveRange(orderToDelete.OrderItems);
                        _context.Orders.Remove(orderToDelete);
                        // Also delete any invoices associated?
                        var invoices = await _context.Invoices.Where(i => i.OrderID == id).ToListAsync();
                        _context.Invoices.RemoveRange(invoices);
                    }
                    else
                    {
                        _logger.LogWarning("DeleteOrder: Order ID {OrderId} not found either in archive or active orders.", id);
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Order or its archive record not found." });
                    }

                }
                else
                {
                    _logger.LogInformation("DeleteOrder: Found archive history for OrderID {OrderId}. ArchivedOrderID: {ArchivedOrderId}", id, archiveHistory.ArchivedOrderID);

                    // Delete ArchivedOrderItems
                    if (archiveHistory.ArchivedOrder != null && archiveHistory.ArchivedOrder.ArchivedOrderItems.Any())
                    {
                        _context.ArchivedOrderItems.RemoveRange(archiveHistory.ArchivedOrder.ArchivedOrderItems);
                        _logger.LogInformation("DeleteOrder: Removed {Count} ArchivedOrderItems for ArchivedOrderID {ArchivedOrderId}",
                           archiveHistory.ArchivedOrder.ArchivedOrderItems.Count, archiveHistory.ArchivedOrderID);
                    }

                    // Delete ArchivedOrder
                    if (archiveHistory.ArchivedOrder != null)
                    {
                        _context.ArchivedOrders.Remove(archiveHistory.ArchivedOrder);
                        _logger.LogInformation("DeleteOrder: Removed ArchivedOrder with ID {ArchivedOrderId}", archiveHistory.ArchivedOrderID);
                    }

                    // Delete ArchiveHistory record itself
                    _context.ArchiveHistories.Remove(archiveHistory);
                    _logger.LogInformation("DeleteOrder: Removed ArchiveHistory for OrderID {OrderId}", id);

                    // Optionally, delete the original Order record if it's still present and marked IsArchived
                    var originalOrder = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderID == id);
                    if (originalOrder != null)
                    {
                        if (originalOrder.IsArchived)
                        {
                            _context.OrderItems.RemoveRange(originalOrder.OrderItems); // Remove its items first
                            _context.Orders.Remove(originalOrder);
                            _logger.LogInformation("DeleteOrder: Removed original (archived) Order with ID {OrderId}", id);
                        }
                        else
                        {
                            _logger.LogWarning("DeleteOrder: Original Order ID {OrderId} exists but is not marked as archived. It was not deleted.", id);
                        }
                    }
                }


                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("DeleteOrder: Successfully deleted records related to original OrderID {OrderId}", id);
                return Json(new { success = true, message = "Archived order and related records deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error while deleting archived order for OriginalID {OrderId}: {Message}. Inner: {InnerMessage}", id, ex.Message, ex.InnerException?.Message);
                return Json(new { success = false, message = "Database error during deletion. Please try again." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting archived order for OriginalID {OrderId}: {Message}", id, ex.Message);
                return Json(new { success = false, message = "Error deleting archived order. Please try again." });
            }
        }


        private async Task ReloadProductsForView()
        {
            ViewBag.Products = await _context.Products
                .Include(p => p.Description)
                .Where(p => p.Quantity != null && p.Quantity.Qty > 0)
                .OrderBy(p => p.ItemName)
                .ToListAsync();
        }

        private string BuildShippingAddress(IFormCollection form)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(form["HouseNo"])) parts.Add(form["HouseNo"].ToString().Trim());
            if (!string.IsNullOrEmpty(form["StreetAddress"])) parts.Add(form["StreetAddress"].ToString().Trim());
            if (!string.IsNullOrEmpty(form["Barangay"])) parts.Add(form["Barangay"].ToString().Trim());
            if (!string.IsNullOrEmpty(form["City"])) parts.Add(form["City"].ToString().Trim());
            if (!string.IsNullOrEmpty(form["PostalId"])) parts.Add(form["PostalId"].ToString().Trim()); // Assuming PostalId is Zip Code

            return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
        }
    }
}