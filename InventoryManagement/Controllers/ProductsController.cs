using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
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
            };

            var viewModel = new ProductViewModel
            {
                Products = products,
                TotalProducts = products.Count,
                SortBy = "A-Z"
            };

            return View(viewModel);
        }
    }
}
