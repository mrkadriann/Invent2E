namespace InventoryManagement.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string StockStatus { get; set; } // "normal", "low", "danger", "overstocked"
        public string ImageUrl { get; set; }
    }
}