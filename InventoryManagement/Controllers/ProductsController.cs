using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;
using InventoryManagement.Data;
using Microsoft.EntityFrameworkCore; 
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Security.Claims;

namespace InventoryManagement.Controllers
{
    public class ProductsController : Controller
    {
        private const string DefaultSortOrder = "NameAsc";
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string sortBy = DefaultSortOrder,
            string categoryFilter = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string stockStatusFilter = null
            )
        {
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Description)
                .Include(p => p.Quantity)
                .Include(p => p.PrimaryImage);

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter.ToLower() != "all")
            {
                string lowerCategoryFilter = categoryFilter.ToLower();
                productsQuery = productsQuery.Where(p =>
                   p.Category != null && p.Category.CategoryName.ToLower() == lowerCategoryFilter);
            }

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Description != null && p.Description.RetailPrice.HasValue && p.Description.RetailPrice.Value >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Description != null && p.Description.RetailPrice.HasValue && p.Description.RetailPrice.Value <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(stockStatusFilter) && stockStatusFilter.ToLower() != "all")
            {
                productsQuery = productsQuery.Where(p => p.Quantity != null);
                switch (stockStatusFilter.ToLower())
                {
                    case "danger":
                        productsQuery = productsQuery.Where(p => p.Quantity.Qty <= 20);
                        break;
                    case "low":
                        productsQuery = productsQuery.Where(p => p.Quantity.Qty > 20 && p.Quantity.Qty <= 30);
                        break;
                    case "normal":
                        productsQuery = productsQuery.Where(p => p.Quantity.Qty > 30 && p.Quantity.Qty <= 50);
                        break;
                    case "overstocked":
                        productsQuery = productsQuery.Where(p => p.Quantity.Qty > 50);
                        break;
                }
            }

            string effectiveSortBy = string.IsNullOrEmpty(sortBy) ? DefaultSortOrder : sortBy.ToLowerInvariant();
            switch (effectiveSortBy)
            {
                case "namedesc":
                    productsQuery = productsQuery.OrderByDescending(p => p.ItemName);
                    break;
                case "priceasc":
                    productsQuery = productsQuery.OrderBy(p => (p.Description != null && p.Description.RetailPrice.HasValue) ? p.Description.RetailPrice.Value : decimal.MaxValue);
                    break;
                case "pricedesc":
                    productsQuery = productsQuery.OrderByDescending(p => (p.Description != null && p.Description.RetailPrice.HasValue) ? p.Description.RetailPrice.Value : decimal.MinValue);
                    break;
                case "categoryasc":
                    productsQuery = productsQuery.OrderBy(p => p.Category != null ? p.Category.CategoryName : string.Empty)
                                                 .ThenBy(p => p.ItemName);
                    break;
                case "categorydesc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Category != null ? p.Category.CategoryName : string.Empty)
                                                 .ThenBy(p => p.ItemName);
                    break;
                case "stockasc":
                    productsQuery = productsQuery.OrderBy(p => p.Quantity != null ? p.Quantity.Qty : int.MaxValue);
                    break;
                case "stockdesc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Quantity != null ? p.Quantity.Qty : int.MinValue);
                    break;
                case "nameasc":
                default:
                    productsQuery = productsQuery.OrderBy(p => p.ItemName);
                    effectiveSortBy = DefaultSortOrder;
                    break;
            }

            List<Product> finalProductsList = await productsQuery.ToListAsync();

            var productViewModels = finalProductsList.Select(p => new ProductForView // Assuming ProductForView is defined elsewhere
            {
                Id = p.ItemId,
                Name = p.ItemName,
                Category = p.Category?.CategoryName,
                Price = p.Description?.RetailPrice,
                Stock = p.Quantity?.Qty,
                StockStatus = CalculateStockStatus(p.Quantity?.Qty),
                ImageUrl = p.PrimaryImageId.HasValue && p.PrimaryImage != null ?
                           $"/Image/GetImage/{p.PrimaryImage.ImageId}" : "/images/placeholder.jpg"
            }).ToList();

            var viewModel = new ProductViewModel 
            {
                Products = productViewModels,
                TotalProducts = productViewModels.Count(),
                SortBy = effectiveSortBy,
                Category = categoryFilter,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                StockStatus = stockStatusFilter,
                AvailableCategories = await _context.Categories
                                            .OrderBy(c => c.CategoryName)
                                            .Select(c => c.CategoryName)
                                            .Distinct()
                                            .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Products/AddProduct
        public async Task<IActionResult> AddProduct()
        {
            var viewModel = new AddProductViewModel
            {
                ProductCategories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .OrderBy(c => c.Text)
                .ToListAsync(),

                Suppliers = await _context.Suppliers
                .OrderBy(s => s.CompanyName)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.CompanyName
                })
                .ToListAsync()
            };
            viewModel.ProductCategories.Insert(0, new SelectListItem { Value = "", Text = "Select Category..." });
            viewModel.Suppliers.Insert(0, new SelectListItem { Value = "", Text = "Select Supplier..." });
            return View(viewModel);
        }

        // POST: Products/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateDropdownsForViewModel(model);
                return View(model);
            }

            var product = new Product
            {
                ItemName = model.ItemName,
                //Supplier = model.SelectedSupplierName ?? "N/A",
                SupplierId = model.SelectedSupplierId,
                CategoryId = model.SelectedProductCategoryId,
                AllImages = new List<ImageData>()
            };

            var description = new Description
            {
                Product = product, // Link back to the product
                DescriptionText = model.ProductDescriptionText ?? string.Empty,
                Height = model.HeightValue.HasValue ? $"{model.HeightValue} {model.HeightUnit}" : string.Empty, // Added space
                Width = model.WidthValue.HasValue ? $"{model.WidthValue} {model.WidthUnit}" : string.Empty,   // Corrected operator and added space
                Weight = model.WeightValue.HasValue ? $"{model.WeightValue} {model.WeightUnit}" : string.Empty, // Corrected to use WeightValue/Unit and added space
                Color = model.Color ?? string.Empty,
                WholesalePrice = model.WholesalePrice ?? 0,
                RetailPrice = model.RetailPrice ?? 0,
                Profit = model.Profit ?? 0
            };
            product.Description = description;

            var quantity = new Quantity
            {
                Product = product,
                Qty = model.QuantityInStock
            };
            product.Quantity = quantity;

            if (model.ImageFiles != null && model.ImageFiles.Count > 0)
            {
                byte imageOrderCounter = 1; 
                foreach (var formFile in model.ImageFiles)
                {
                    if (formFile.Length > 0) 
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(memoryStream);
                            var imageData = new ImageData
                            {
                                Product = product,
                                ImageDt = memoryStream.ToArray(),
                                ImageOrder = imageOrderCounter++
                            };
                            product.AllImages.Add(imageData);
                        }
                    }
                }
            }

            _context.Products.Add(product); 

            try
            {
                await _context.SaveChangesAsync();

                if (product.AllImages.Any())
                {
                    var primaryImg = product.AllImages
                                            .OrderBy(img => img.ImageOrder ?? byte.MaxValue)
                                            .ThenBy(img => img.ImageId)
                                            .FirstOrDefault();
                    if (primaryImg != null)
                    {
                        product.PrimaryImageId = primaryImg.ImageId;
                        _context.Products.Update(product);
                        await _context.SaveChangesAsync(); 
                    }
                }

                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error saving product: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, see your system administrator.");
            }

            // If save fails or ModelState was initially invalid, re-populate dropdowns
            await RepopulateDropdownsForViewModel(model);
            return View(model);
        }

        private async Task RepopulateDropdownsForViewModel(AddProductViewModel model)
        {
            model.ProductCategories = await _context.Categories
                                            .Select(c => new SelectListItem
                                            {
                                                Value = c.CategoryId.ToString(),
                                                Text = c.CategoryName
                                            })
                                            .OrderBy(c => c.Text)
                                            .ToListAsync();
            model.ProductCategories.Insert(0, new SelectListItem { Value = "", Text = "Select Category..." });

            model.Suppliers = await _context.Suppliers
                                    .OrderBy(s => s.CompanyName)
                                    .Select(s => new SelectListItem
                                    {
                                        Value = s.SupplierId.ToString(),
                                        Text = s.CompanyName
                                    })
                                    .ToListAsync();
            model.Suppliers.Insert(0, new SelectListItem { Value = "", Text = "Select Supplier...." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustQuantity(AdjustmentViewModel model)
        {
            // --- Start Validation Logic (same as before) ---
            if (model.AdjustmentTypeValue == "Quantity")
            {
                if (!model.AdjustedQuantity.HasValue)
                {
                    ModelState.AddModelError(nameof(model.AdjustedQuantity), "New Quantity is required for a quantity adjustment.");
                }
                else if (model.AdjustedQuantity < 0)
                {
                    ModelState.AddModelError(nameof(model.AdjustedQuantity), "New Quantity cannot be negative.");
                }
            }
            else if (model.AdjustmentTypeValue == "Value")
            {
                if (!model.AdjustmentValueAmount.HasValue)
                {
                    ModelState.AddModelError(nameof(model.AdjustmentValueAmount), "Adjustment Value Amount is required for a value adjustment.");
                }
            }

            if (string.IsNullOrEmpty(model.Reason) || model.Reason == "") // Check for empty string from "Select Reason..."
            {
                ModelState.AddModelError(nameof(model.Reason), "Adjustment reason is required.");
            }
            // --- End Validation Logic ---

            if (!ModelState.IsValid)
            {
                // Repopulate reasons if they were lost or not part of the model by default on POST
                //if (model.AdjustmentReasons == null || !model.AdjustmentReasons.Any())
                //{
                //    model.AdjustmentReasons = new List<SelectListItem>
                //     {
                //        new SelectListItem { Value = "", Text = "Select Reason..." },
                //        new SelectListItem { Value = "Stock Count", Text = "Stock Count" },
                //        new SelectListItem { Value = "Damaged Goods", Text = "Damaged Goods" },
                //        new SelectListItem { Value = "Returned Goods", Text = "Returned Goods" },
                //        new SelectListItem { Value = "Promotion", Text = "Promotion" },
                //        new SelectListItem { Value = "Other", Text = "Other" }
                //     };
                //}
                // Return to the same page with validation errors
                return View("AdjustStockPage", model);
            }

            var product = await _context.Products
                                        .Include(p => p.Quantity)
                                        .FirstOrDefaultAsync(p => p.ItemId == model.ProductId);

            if (product == null)
            {
                ModelState.AddModelError("", "Product not found. Adjustment cannot be saved.");
                // Repopulate reasons for the view
                //if (model.AdjustmentReasons == null || !model.AdjustmentReasons.Any())
                //{
                //    model.AdjustmentReasons = new List<SelectListItem> { /* ... as above ... */ };
                //}
                return View("AdjustStockPage", model);
            }

            // --- Start Logging and Update Logic (same as before) ---
            var adjustmentLog = new AdjustmentLog
            {
                ProductId = product.ItemId,
                AdjustmentDate = model.Date,
                Reason = model.Reason,
                Description = model.Description,
                AdjustmentType = model.AdjustmentTypeValue,
                LogTimestamp = DateTime.UtcNow
            };
            // User ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            adjustmentLog.UserId = !string.IsNullOrEmpty(userIdClaim) ? userIdClaim : (User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(User.Identity.Name) ? User.Identity.Name : "System/Unknown");

            if (product.Quantity == null)
            {
                product.Quantity = new Quantity { ItemId = product.ItemId, Qty = 0, LastUpdated = DateTime.UtcNow };
                _context.Quantities.Add(product.Quantity);
            }
            adjustmentLog.OldQuantity = product.Quantity.Qty;

            if (model.AdjustmentTypeValue == "Quantity" && model.AdjustedQuantity.HasValue)
            {
                product.Quantity.Qty = model.AdjustedQuantity.Value;
                adjustmentLog.NewQuantity = model.AdjustedQuantity.Value;
            }
            else if (model.AdjustmentTypeValue == "Value" && model.AdjustmentValueAmount.HasValue)
            {
                TempData["WarningMessage"] = "Value adjustment processing is a placeholder. Define business logic.";
                // Actual value adjustment logic would go here
            }
            product.Quantity.LastUpdated = DateTime.UtcNow;
            _context.AdjustmentLogs.Add(adjustmentLog);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Stock for '{product.ItemName}' adjusted successfully. Log ID: {adjustmentLog.LogId}";
                return RedirectToAction(nameof(Index)); // Or RedirectToAction("ProductDetail", new { id = product.ItemId });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DB Update Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the adjustment. Please check the details and try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in AdjustQuantity: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again or contact support.");
            }

            //if (model.AdjustmentReasons == null || !model.AdjustmentReasons.Any())
            //{
            //    model.AdjustmentReasons = new List<SelectListItem> { /* ... as above ... */ };
            //}
            return View("AdjustStockPage", model);
        }

        [HttpGet]
        public async Task<IActionResult> AdjustStockPage(int productId)
        {
            var product = await _context.Products
                                        .Include(p => p.Quantity)
                                        .Include(p => p.PrimaryImage)
                                        .FirstOrDefaultAsync(p => p.ItemId == productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index)); // Or a specific error page
            }

            var viewModel = new AdjustmentViewModel
            {
                ProductId = product.ItemId,
                ProductName = product.ItemName,
                CurrentStockQuantity = product.Quantity?.Qty ?? 0,
                StockStatus = CalculateStockStatus(product.Quantity?.Qty),
                AdjustedQuantity = product.Quantity?.Qty ?? 0,
                Date = DateTime.Today,
                AdjustmentTypeValue = "Quantity",
                ProductImageUrl = product.PrimaryImageId.HasValue && product.PrimaryImage != null ?
                                   $"/Image/GetImage/{product.PrimaryImage.ImageId}" : "/images/placeholder-sm.jpg"
            };

            //if (viewModel.AdjustmentReasons == null || !viewModel.AdjustmentReasons.Any())
            //{
            //    viewModel.AdjustmentReasons = new List<SelectListItem>
            //     {
            //        new SelectListItem { Value = "", Text = "Select Reason..." },
            //        new SelectListItem { Value = "Stock Count", Text = "Stock Count" },
            //        new SelectListItem { Value = "Damaged Goods", Text = "Damaged Goods" },
            //        new SelectListItem { Value = "Returned Goods", Text = "Returned Goods" },
            //        new SelectListItem { Value = "Promotion", Text = "Promotion" },
            //        new SelectListItem { Value = "Other", Text = "Other" }
            //     };
            //}


            return View(viewModel);
        }

        private string CalculateStockStatus(int? qty)
        {
            if (!qty.HasValue) return "Unknown";
            if (qty.Value <= 20) return "danger";
            if (qty.Value <= 30) return "low";
            if (qty.Value <= 50) return "normal";
            return "overstocked";
        }

        // GET: Products/GetProductDetail/5
        [HttpGet]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Description)
                .Include(p => p.Quantity)
                .Include(p => p.PrimaryImage)
                .Include(p => p.AllImages)
                .FirstOrDefaultAsync(p => p.ItemId == id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailViewModel
            {
                Id = product.ItemId,
                ProductId = product.ItemId.ToString(), // Or use another field if you have a separate product ID
                Name = product.ItemName,
                Category = product.Category?.CategoryName ?? "Uncategorized",
                ImageUrl = product.PrimaryImageId.HasValue && product.PrimaryImage != null ?
                           $"/Image/GetImage/{product.PrimaryImage.ImageId}" : "/images/placeholder.jpg",
                Stock = product.Quantity?.Qty ?? 0,
                StockStatus = CalculateStockStatus(product.Quantity?.Qty),
                Price = product.Description?.RetailPrice ?? 0,
                RetailPrice = product.Description?.RetailPrice ?? 0,
                WholesalePrice = product.Description?.WholesalePrice ?? 0,
                Profit = product.Description?.Profit ?? 0,
                Supplier = product.SupplierId.ToString(),
                Color = product.Description?.Color,
                Height = product.Description?.Height,
                Width = product.Description?.Width,
                Weight = product.Description?.Weight,
                Description = product.Description?.DescriptionText
            };

            // Add additional images if they exist
            if (product.AllImages != null && product.AllImages.Any())
            {
                viewModel.AdditionalImages = product.AllImages
                    .Where(img => img != null && img.ImageId != product.PrimaryImageId)
                    .OrderBy(img => img.ImageOrder)
                    .Select(img => $"/Image/GetImage/{img.ImageId}")
                    .ToList();
            }

            return PartialView("_ProductDetailPartial", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PerformDelete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Quantity)
                .Include(p => p.Description)
                .FirstOrDefaultAsync(p => p.ItemId == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found or already deleted." });
            }

            try
            {
                // Remove related Quantity if present
                if (product.Quantity != null)
                {
                    _context.Quantities.Remove(product.Quantity);
                }

                // Remove related Quantity rows
                if (product.Quantity != null)
                {
                    _context.Quantities.Remove(product.Quantity);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Product '{product.ItemName}' deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                var baseMessage = ex.GetBaseException().Message;
                return Json(new { success = false, message = $"Unable to delete product. DB error: {baseMessage}" });
            }
        }

    }
}


