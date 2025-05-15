using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;
using InventoryManagement.Data;     // For InventoryDbContext
using Microsoft.EntityFrameworkCore; // For Include, ToListAsync, etc.
using System.Linq;                 // For LINQ methods (Where, OrderBy, etc.)
using System.Threading.Tasks;      // For async operations
using System.Collections.Generic;

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
            String sortBy =  DefaultSortOrder,
            String categoryFilter = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string stockStatusFilter = null
            )
        {
            // This would normally come from a database
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Description)
                .Include(p => p.Quantity)
                .Include(p => p.PrimaryImage);

            // --- Applying Filter ---

            if(!string.IsNullOrEmpty(categoryFilter) && categoryFilter.ToLower() != "all")
            {
                productsQuery = productsQuery.Where(p =>
                   p.Category != null && p.Category.CategoryName.Equals(categoryFilter, System.StringComparison.OrdinalIgnoreCase));
            }

            // --- Filter Minimun Price ---

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Description != null && p.Description.RetailPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Description != null && p.Description.RetailPrice <= maxPrice.Value);
            }

            // --- Filter by Stock ---

            if (!string.IsNullOrEmpty(stockStatusFilter) && stockStatusFilter.ToLower() != "all")
            {
                productsQuery = productsQuery.Where(p => p.Quantity != null); // Ensure quantity object exists
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
            // --- End of Filtering ---

            // --- Apply Sorting --- 
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
                case "nameasc": // Default case
                default:
                    productsQuery = productsQuery.OrderBy(p => p.ItemName);
                    effectiveSortBy = DefaultSortOrder; // Ensure the view model gets the actual applied sort order
                    break;
            }
            // --- End of Sorting ---

            List<Product> finalProductsList = await productsQuery.ToListAsync();

            var productViewModels = finalProductsList.Select(p => new ProductForView
            {
                Id = p.ItemId,
                Name = p.ItemName,
                Category = p.Category?.CategoryName, // Safe navigation
                Price = p.Description?.RetailPrice,  // Safe navigation
                Stock = p.Quantity?.Qty,             // Safe navigation
                StockStatus = CalculateStockStatus(p.Quantity?.Qty), // Helper method
                ImageUrl = p.PrimaryImageId.HasValue && p.PrimaryImage != null ?
                           $"/Image/GetImage/{p.PrimaryImage.ImageId}" : "/images/placeholder.jpg" // Assumes an ImageController
            }).ToList();

            var viewModel = new ProductViewModel
            {
                Products = productViewModels,
                TotalProducts = productViewModels.Count(), // Count of the filtered and transformed items
                SortBy = effectiveSortBy, // Pass the applied sort order
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

        private string CalculateStockStatus(int? qty)
        {
            if (!qty.HasValue) return "Unknown"; 
            if (qty.Value <= 20) return "danger";
            if (qty.Value <= 30) return "low";
            if (qty.Value <= 50) return "normal";
            return "overstocked";
        }
    }
}
