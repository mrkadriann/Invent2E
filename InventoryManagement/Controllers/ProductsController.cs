using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ProductsController : Controller
    {
        private const string DefaultSortOrder = "NameAsc";
        public IActionResult Index(
            String sortBy =  DefaultSortOrder,
            String category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string stockStatus = null
            )
        {
            // This would normally come from a database
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Aliodas Shirt", Category = "Women's Clothes", Price = 2000, Stock = 30, StockStatus = "danger", ImageUrl = "/images/products/aliodas-shirt.jpg" },
                new Product { Id = 2, Name = "What the Sigma Shirt", Category = "Men's Clothes", Price = 1500, Stock = 1000, StockStatus = "overstocked", ImageUrl = "/images/products/sigma-shirt.jpg" },
                new Product { Id = 3, Name = "New Balance Men's 608 V5", Category = "Men's Shoes", Price = 2000, Stock = 100, StockStatus = "normal", ImageUrl = "/images/products/nb-608.jpg" },
                new Product { Id = 4, Name = "Unisex Satin Luxe Sneakers", Category = "Unisex Shoes", Price = 3000, Stock = 200, StockStatus = "normal", ImageUrl = "/images/products/satin-luxe.jpg" },
                new Product { Id = 5, Name = "SAMBA OG SHOES", Category = "Women's Shoes", Price = 2000, Stock = 30, StockStatus = "low", ImageUrl = "/images/products/samba-og.jpg" },
                new Product { Id = 6, Name = "Nike m2k tekno", Category = "Unisex Shoes", Price = 2500, Stock = 30, StockStatus = "low", ImageUrl = "/images/products/m2k-tekno.jpg" },
                new Product { Id = 7, Name = "Adidas Forum", Category = "Men's Shoes", Price = 3000, Stock = 200, StockStatus = "normal", ImageUrl = "/images/products/adidas-forum.jpg" },
                new Product { Id = 8, Name = "LV Trainer Sneaker Grey", Category = "Men's Shoes", Price = 5000, Stock = 30, StockStatus = "danger", ImageUrl = "/images/products/lv-trainer.jpg" },
                new Product { Id = 9, Name = "LV Trainer Sneaker Grey", Category = "Men's Shoes", Price = 5000, Stock = 30, StockStatus = "danger", ImageUrl = "/images/products/lv-trainer.jpg" },
                new Product { Id = 10, Name = "LV Trainer Sneaker Grey", Category = "Men's Shoes", Price = 5000, Stock = 30, StockStatus = "danger", ImageUrl = "/images/products/lv-trainer.jpg" },
            };

            IEnumerable<Product> filteredProducts = products.AsEnumerable();

            // --- Applying Filter ---

            if(!string.IsNullOrEmpty(category) && category.ToLower() != "all")
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.Category != null && p.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase));
            }

            // --- Filter Minimun Price ---

            if (minPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);
            }

            // --- Filter by Stock ---

            if (!string.IsNullOrEmpty(stockStatus) && stockStatus.ToLower() != "all")
            {
                filteredProducts = filteredProducts.Where(p =>
                p.StockStatus != null && p.StockStatus.Equals(stockStatus,
                System.StringComparison.OrdinalIgnoreCase));
            }
            // --- End of Filtering ---

            // --- Apply Sorting --- 
            switch (sortBy?.ToLowerInvariant())
            {
                case "namedesc":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Name);
                    break;
                case "priceasc":
                    filteredProducts = filteredProducts.OrderBy(p => p.Price);
                    break;
                case "pricedesc":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Price);
                    break;
                case "categoryasc":
                    filteredProducts = filteredProducts.OrderBy(p => p.Category).ThenBy(p => p.Name);
                    break;
                case "categorydesc":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Category).ThenBy(p => p.Name);
                    break;
                case "stockasc":
                    filteredProducts = filteredProducts.OrderBy(p => p.Stock);
                    break;
                case "stockdesc":
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Stock);
                    break;
                case "nameasc":
                default:
                    filteredProducts = filteredProducts.OrderBy(p => p.Name);
                    sortBy = DefaultSortOrder;
                    break;
            } // --- End of Sorting ---


            var viewModel = new ProductViewModel
            {
                Products = filteredProducts.ToList(),
                TotalProducts = filteredProducts.Count(),
                SortBy = sortBy,
                Category = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                StockStatus = stockStatus,
                AvailableCategories = products
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList()
            };

            return View(viewModel);
        }

        public ActionResult Add()
        {
            return View("CreateProduct");
        }
    }
}
