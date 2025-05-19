// Sa InventoryManagement/Models/Settings.cs
namespace InventoryManagement.Models
{
    public class Settings
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? BusinessType { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Language { get; set; }
        public string? Currency { get; set; }
        public bool EnableTax { get; set; }
        public decimal? TaxRate { get; set; }
        public bool NotifyLowStock { get; set; }
        public bool NotifyOutOfStock { get; set; }
        public bool NotifyReplenish { get; set; }
        public string? Employee { get; set; }
        public string? Customer { get; set; }
        public bool DisableFeature1 { get; set; }
        public bool DisableFeature2 { get; set; }
        public string? CompanyPhotoPath { get; set; }
    }
}