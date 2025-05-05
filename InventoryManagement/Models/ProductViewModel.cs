namespace InventoryManagement.Models
{
    public class ProductViewModel
    {
        public List<Product> Products { get; set; }
        public int TotalProducts { get; set; }
        public string SortBy { get; set; }
        public string Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string StockStatus { get; set; }

        public List<string>  AvailableCategories { get; set; } = new List<string>();

        public List<string> AvailableStockStatuses { get; set; } = new List<string> { "All", "normal", "low", "danger", "overstocked" };

    }
}
