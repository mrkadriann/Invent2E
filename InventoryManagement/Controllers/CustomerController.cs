using InventoryManagement.Data;
using InventoryManagement.Models; // Assuming CustomerViewModel is here
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // For IFormFile
using System.IO; // For MemoryStream
using System.Linq; // For LINQ methods
using System.Threading.Tasks; // For async operations
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace InventoryManagement.Controllers

{
    public class CustomerController : Controller
    {
        private readonly InventoryDbContext _context;

        // You might want to define these if you have fixed types/statuses
        private readonly List<string> _predefinedCustomerTypes = new List<string> { "Retail", "Wholesale", "VIP" };
        private readonly List<string> _predefinedCustomerStatuses = new List<string> { "Active", "Inactive", "Pending" };

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public CustomerController(InventoryDbContext context,
                                  IUrlHelperFactory urlHelperFactory,
                                  IActionContextAccessor actionContextAccessor)
        {
            _context = context;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        // GET: Customer
        public async Task<IActionResult> Index(string sortOrder, string searchString, string typeFilter, string statusFilter, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParam"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["TypeSortParam"] = sortOrder == "Type" ? "type_desc" : "Type";

            // For persistence of search/filter across pagination
            ViewData["CurrentSearchFilter"] = searchString;
            ViewData["CurrentTypeFilter"] = typeFilter;
            ViewData["CurrentStatusFilter"] = statusFilter;

            var customersQuery = _context.Customers
                                         .Include(c => c.Information) // Eager load customer information
                                         .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearchString = searchString.ToLower().Trim();
                customersQuery = customersQuery.Where(c =>
                    (c.CustomerName != null && c.CustomerName.ToLower().Contains(lowerSearchString)) ||
                    (c.Information != null && c.Information.CustomerEmail != null && c.Information.CustomerEmail.ToLower().Contains(lowerSearchString)) ||
                    (c.Information != null && c.Information.PhoneNumber != null && c.Information.PhoneNumber.Contains(lowerSearchString))
                );
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                customersQuery = customersQuery.Where(c => c.CustomerType == typeFilter);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                customersQuery = customersQuery.Where(c => c.CustomerStatus == statusFilter);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    customersQuery = customersQuery.OrderByDescending(c => c.CustomerName);
                    break;
                case "Email":
                    customersQuery = customersQuery.OrderBy(c => c.Information.CustomerEmail);
                    break;
                case "email_desc":
                    customersQuery = customersQuery.OrderByDescending(c => c.Information.CustomerEmail);
                    break;
                case "Type":
                    customersQuery = customersQuery.OrderBy(c => c.CustomerType);
                    break;
                case "type_desc":
                    customersQuery = customersQuery.OrderByDescending(c => c.CustomerType);
                    break;
                default: // Default sort by name
                    customersQuery = customersQuery.OrderBy(c => c.CustomerName);
                    break;
            }

            ViewBag.CustomerTypes = _predefinedCustomerTypes.OrderBy(t => t).ToList();
            ViewBag.CustomerStatuses = _predefinedCustomerStatuses.OrderBy(s => s).ToList();

            // Basic pagination example (you might use a library like X.PagedList)
            int pageSize = 10;
            var paginatedCustomers = await PaginatedList<Customer>.CreateAsync(customersQuery.AsNoTracking(), pageNumber ?? 1, pageSize);
            
            ViewData["Title"] = "Customers";
            return View(paginatedCustomers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Information)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer); // You'd create a Details.cshtml view for this
        }


        // GET: Customer/GetCustomerPicture/5
        public async Task<IActionResult> GetCustomerPicture(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer != null && customer.CustomerPicture != null && customer.CustomerPicture.Length > 0)
            {
                // Basic content type detection (you might want a more robust method)
                string contentType = "image/jpeg"; // Default
                if (IsPng(customer.CustomerPicture)) contentType = "image/png";
                // Add more for gif, webp etc. if needed

                return File(customer.CustomerPicture, contentType);
            }
            // Return a default placeholder image if you have one
            // return File("~/images/default-customer.png", "image/png");
            return NotFound();
        }

        private bool IsPng(byte[] bytes)
        {
            return bytes.Length > 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }


        // GET: Customer/Create
        public IActionResult Create()
        {
            ViewBag.CustomerTypes = _predefinedCustomerTypes;
            ViewBag.CustomerStatuses = _predefinedCustomerStatuses;
            var viewModel = new CustomerViewModel();
            return View(viewModel);
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate email if it should be unique
                if (await _context.CustomerInformations.AnyAsync(ci => ci.CustomerEmail == viewModel.CustomerEmail))
                {
                    ModelState.AddModelError("CustomerEmail", "This email address is already in use.");
                    ViewBag.CustomerTypes = _predefinedCustomerTypes;
                    ViewBag.CustomerStatuses = _predefinedCustomerStatuses;
                    return View(viewModel);
                }

                var customer = new Customer
                {
                    CustomerName = viewModel.CustomerName,
                    CustomerType = viewModel.CustomerType,
                    CustomerStatus = viewModel.CustomerStatus,
                    CustomerRemark = viewModel.CustomerRemark,
                    // CustomerPicture will be handled below
                };

                var customerInfo = new CustomerInformation
                {
                    PhoneNumber = viewModel.PhoneNumber,
                    CustomerEmail = viewModel.CustomerEmail,
                    ShippingAddress = viewModel.ShippingAddress,
                    BillingAddress = viewModel.BillingAddress,
                    // CustomerId will be set by EF Core relationship or manually after Customer save
                    Customer = customer // Link information to customer
                };

                customer.Information = customerInfo; // Establish the one-to-one link

                if (viewModel.CustomerPictureFile != null && viewModel.CustomerPictureFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.CustomerPictureFile.CopyToAsync(memoryStream);
                        customer.CustomerPicture = memoryStream.ToArray();
                    }
                }

                _context.Customers.Add(customer); // EF Core will also add customerInfo due to the relationship
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerTypes = _predefinedCustomerTypes;
            ViewBag.CustomerStatuses = _predefinedCustomerStatuses;
            return View(viewModel);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                                       .Include(c => c.Information) // Eager load
                                       .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }
            if (customer.Information == null) // Should not happen if DB is consistent, but good for robustness
            {
                // Handle case where information might be missing, perhaps initialize it
                // For now, let's assume it exists or redirect/error out
                TempData["ErrorMessage"] = "Customer information is missing. Cannot edit.";
                return RedirectToAction(nameof(Index));
            }


            var viewModel = new CustomerViewModel
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                CustomerType = customer.CustomerType,
                CustomerStatus = customer.CustomerStatus,
                CustomerRemark = customer.CustomerRemark,
                HasExistingPicture = customer.CustomerPicture != null && customer.CustomerPicture.Length > 0,

                CustomerInfoId = customer.Information.CustomerInfoId,
                PhoneNumber = customer.Information.PhoneNumber,
                CustomerEmail = customer.Information.CustomerEmail,
                ShippingAddress = customer.Information.ShippingAddress,
                BillingAddress = customer.Information.BillingAddress
            };

            ViewBag.CustomerTypes = _predefinedCustomerTypes;
            ViewBag.CustomerStatuses = _predefinedCustomerStatuses;
            return View(viewModel);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerViewModel viewModel)
        {
            if (id != viewModel.CustomerId)
            {
                return BadRequest("Mismatched customer ID.");
            }

            // If no new image is uploaded, don't validate CustomerPictureFile
            if (viewModel.CustomerPictureFile == null)
            {
                ModelState.Remove(nameof(viewModel.CustomerPictureFile));
            }

            if (ModelState.IsValid)
            {
                var customerToUpdate = await _context.Customers
                                                   .Include(c => c.Information) // Important to load related data
                                                   .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customerToUpdate == null)
                {
                    return NotFound();
                }

                // Check for duplicate email (excluding the current customer's email)
                if (await _context.CustomerInformations.AnyAsync(ci => ci.CustomerEmail == viewModel.CustomerEmail && ci.CustomerId != id))
                {
                    ModelState.AddModelError("CustomerEmail", "This email address is already in use by another customer.");
                    ViewBag.CustomerTypes = _predefinedCustomerTypes;
                    ViewBag.CustomerStatuses = _predefinedCustomerStatuses;
                    viewModel.HasExistingPicture = customerToUpdate.CustomerPicture != null && customerToUpdate.CustomerPicture.Length > 0;
                    return View(viewModel);
                }


                customerToUpdate.CustomerName = viewModel.CustomerName;
                customerToUpdate.CustomerType = viewModel.CustomerType;
                customerToUpdate.CustomerStatus = viewModel.CustomerStatus;
                customerToUpdate.CustomerRemark = viewModel.CustomerRemark;

                // Ensure Information entity exists to update it
                if (customerToUpdate.Information == null)
                {
                    // This case should ideally not happen if data integrity is maintained.
                    // If it can, you might need to create a new CustomerInformation here.
                    // For simplicity, we'll assume it exists based on the GET Edit load.
                    customerToUpdate.Information = new CustomerInformation { CustomerId = customerToUpdate.CustomerId };
                    _context.CustomerInformations.Add(customerToUpdate.Information); // Add to context if newly created
                }

                customerToUpdate.Information.PhoneNumber = viewModel.PhoneNumber;
                customerToUpdate.Information.CustomerEmail = viewModel.CustomerEmail;
                customerToUpdate.Information.ShippingAddress = viewModel.ShippingAddress;
                customerToUpdate.Information.BillingAddress = viewModel.BillingAddress;

                if (viewModel.CustomerPictureFile != null && viewModel.CustomerPictureFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await viewModel.CustomerPictureFile.CopyToAsync(memoryStream);
                        customerToUpdate.CustomerPicture = memoryStream.ToArray();
                    }
                    viewModel.HasExistingPicture = true; // Update flag
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customerToUpdate.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "The record was modified by another user. Please try again.");
                        // Re-fetch or show current DB values if needed
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerTypes = _predefinedCustomerTypes;
            ViewBag.CustomerStatuses = _predefinedCustomerStatuses;
            // Repopulate HasExistingPicture if returning to the view due to validation errors
            if (viewModel.CustomerId > 0)
            {
                var originalCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == viewModel.CustomerId);
                if (originalCustomer != null)
                {
                    viewModel.HasExistingPicture = originalCustomer.CustomerPicture != null && originalCustomer.CustomerPicture.Length > 0;
                }
            }
            return View(viewModel);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Information)
                .AsNoTracking() // No need to track for delete confirmation view
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Delete Customer";
            return View(customer); // Pass the full Customer model to the Delete confirmation view
        }

        // POST: Customer/Delete/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers
                                       .Include(c => c.Information) // Include Information to ensure it's deleted by cascade
                                       .FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // If you have OnDelete(DeleteBehavior.Cascade) for CustomerInformation from Customer,
                // removing the customer will also remove their information.
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            catch (DbUpdateException ex)
            {
                // Log the error (ex)
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.GetBaseException().Message}. It might be referenced by other records.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        public async Task<IActionResult> GetCustomerDetailsPartial(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Information) // Eager load related information
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }
            if (customer.Information == null)
            {
                // This can happen if data is inconsistent.
                // Initialize a blank one to prevent null reference errors in the view.
                customer.Information = new CustomerInformation();
            }


            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var viewModel = new CustomerDetailViewModel(customer, urlHelper);

            return PartialView("_CustomerDetail", viewModel);
        }
    }
       
    // Helper class for pagination (can be in a separate file)
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

    }
}