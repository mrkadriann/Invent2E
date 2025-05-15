namespace InventoryManagement.Models // Or InventoryManagement.ViewModels
{
    public class ProductForView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string StockStatus { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProductViewModel
    {
        public List<ProductForView> Products { get; set; }
        public int TotalProducts { get; set; }
        public string SortBy { get; set; }
        public string Category { get; set; } // This stores the selected category filter
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string StockStatus { get; set; } // This stores the selected stock status filter
        public List<string> AvailableCategories { get; set; }

        public ProductViewModel()
        {
            Products = new List<ProductForView>();
            AvailableCategories = new List<string>();
        }
    }
}
