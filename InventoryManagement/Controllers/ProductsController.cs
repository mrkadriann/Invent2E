using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;
using InventoryManagement.Data;     // For InventoryDbContext
using Microsoft.EntityFrameworkCore; // For Include, ToListAsync, etc.
using System.Linq;                 // For LINQ methods (Where, OrderBy, etc.)
using System.Threading.Tasks;      // For async operations
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem
using System.IO;                         // For MemoryStream

namespace InventoryManagement.Controllers
{
    public class ProductsController : Controller
    {
        private const string DefaultSortOrder = "NameAsc";
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context) // Constructor Injection
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
                productsQuery = productsQuery.Where(p => p.Description != null && p.Description.RetailPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Description != null && p.Description.RetailPrice <= maxPrice.Value);
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
                    productsQuery = productsQuery.OrderBy(p => p.Description != null ? p.Description.RetailPrice : decimal.MaxValue);
                    break;
                case "pricedesc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Description != null ? p.Description.RetailPrice : decimal.MinValue);
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

            var viewModel = new ProductViewModel // Assuming ProductViewModel is defined elsewhere for the Index page
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

                Suppliers = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select Supplier..."},
                    new SelectListItem { Value = "Supplier Alpha", Text = "Supplier Alpha"}, // Corrected
                    new SelectListItem { Value = "Beta Components", Text = "Beta Components"},
                    new SelectListItem { Value = "Gamma Wholesale", Text = "Gamma Wholesale"} // Corrected
                }
            };
            viewModel.ProductCategories.Insert(0, new SelectListItem { Value = "", Text = "Select Category..." });
            return View(viewModel);
        }

        // POST: Products/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateDropdownsForViewModel(model); // Corrected method name
                return View(model);
            }

            var product = new Product
            {
                ItemName = model.ItemName,
                Supplier = model.SelectedSupplierName ?? "N/A",
                CategoryId = model.SelectedProductCategoryId,
                AllImages = new List<ImageData>() // Initialize collection
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
                Profit = model.Profit ?? 0 // Corrected missing '='
            };
            product.Description = description; // Assign to navigation property (lowercase 'product')

            var quantity = new Quantity
            {
                Product = product, // Link back (lowercase 'product')
                Qty = model.QuantityInStock
            };
            product.Quantity = quantity; // Assign to navigation property (lowercase 'product')

            // Handle Image Uploads (convert to byte[])
            if (model.ImageFiles != null && model.ImageFiles.Count > 0)
            {
                byte imageOrderCounter = 1; // Corrected '1l' to '1'
                foreach (var formFile in model.ImageFiles)
                {
                    if (formFile.Length > 0) // Corrected 'FormFile.lenght' to 'formFile.Length'
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(memoryStream); // Corrected 'CopyToAysnc'
                            var imageData = new ImageData
                            {
                                Product = product, // Link back to the product
                                ImageDt = memoryStream.ToArray(),
                                ImageOrder = imageOrderCounter++
                                // ItemId will be set by EF Core due to Product navigation property
                            };
                            product.AllImages.Add(imageData);
                        }
                    }
                }
            }

            _context.Products.Add(product); // Add product (and its related entities) to the context

            try
            {
                await _context.SaveChangesAsync(); // Save all changes to the database

                // After saving product and its images, if there are images, set the PrimaryImageId
                if (product.AllImages.Any())
                {
                    // Select the first image (ordered by ImageOrder, then by ImageId as a tie-breaker) as primary
                    var primaryImg = product.AllImages
                                            .OrderBy(img => img.ImageOrder ?? byte.MaxValue)
                                            .ThenBy(img => img.ImageId)
                                            .FirstOrDefault();
                    if (primaryImg != null)
                    {
                        product.PrimaryImageId = primaryImg.ImageId;
                        _context.Products.Update(product); // Mark product as modified to save PrimaryImageId
                        await _context.SaveChangesAsync(); // Save the PrimaryImageId update
                    }
                }

                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log the error (ex.Message or ex.InnerException.Message for more details)
                // For production, use a proper logging framework
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

            model.Suppliers = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select Supplier..." },
                new SelectListItem { Value = "Supplier Alpha", Text = "Supplier Alpha" }, // Corrected
                new SelectListItem { Value = "Beta Components", Text = "Beta Components" },
                new SelectListItem { Value = "Gamma Wholesale", Text = "Gamma Wholesale" }  // Corrected
            };
            // DimensionUnits and WeightUnits are initialized in AddProductViewModel's constructor
            // and should persist on the model if it's returned to the view.
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
            // Fetch product with all related data
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

            // Create a view model with all the necessary details
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
                Supplier = product.Supplier ?? "N/A",
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
    }
}