using System.Collections.Generic;

namespace InventoryManagement.Models
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public List<string> AdditionalImages { get; set; }
        public int Stock { get; set; }
        public string StockStatus { get; set; }
        public decimal Price { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal Profit { get; set; }
        public string Supplier { get; set; }
        public string Color { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Weight { get; set; }
        public string Description { get; set; }
    }
}