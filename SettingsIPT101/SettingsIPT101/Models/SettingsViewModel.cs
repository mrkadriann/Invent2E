// Models/SettingsViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SettingsIPT101.Models
{
    public class SettingsViewModel
    {
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Business Type")]
        public string BusinessType { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone no.")]
        public string Phone { get; set; }

        public string Address { get; set; }

        [Display(Name = "Language")]
        public string Language { get; set; }

        [Display(Name = "Enable Tax")]
        public bool EnableTax { get; set; }

        [Range(0, 100)]
        [Display(Name = "Tax Rate (%)")]
        public decimal? TaxRate { get; set; }

        [Display(Name = "Notify when the product is LOW in stocks")]
        public bool NotifyLowStock { get; set; }

        [Display(Name = "Notify when the product is OUT of stocks")]
        public bool NotifyOutOfStock { get; set; }

        [Display(Name = "Notify when product needs REPLENISHMENT")]
        public bool NotifyReplenish { get; set; }

        // Existing properties
        [Display(Name = "Employee")]
        public string Employee { get; set; }

        [Display(Name = "Customer")]
        public string Customer { get; set; }

        [Display(Name = "Disable Feature 1")]
        public bool DisableFeature1 { get; set; }

        [Display(Name = "Disable Feature 2")]
        public bool DisableFeature2 { get; set; }

        [Display(Name = "Company Photo")]
        public IFormFile CompanyPhoto { get; set; }

        public string CurrentPhotoUrl { get; set; }

        [Display(Name = "Currency")]
        public string Currency { get; set; }
    }
}
