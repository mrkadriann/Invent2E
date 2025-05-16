using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Models
{
    public class AddSupplierViewModel
    {
        [Required(ErrorMessage = "Company Name is required.")]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile ProfileImageFile { get; set; } // For uploading the image

        [StringLength(100)]
        [Display(Name = "Contact Person Name")]
        public string PersonName { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Currency { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string Courier { get; set; }

        [Required(ErrorMessage = "Portal Status is required.")]
        [StringLength(20)]
        [Display(Name = "Portal Status")]
        public string PortalStatus { get; set; } = "Active"; // Default value

        // Optional: If you want to add a primary contact person at the same time
        [Display(Name = "Primary Contact: Name")]
        public string PrimaryContactName { get; set; }
        [Display(Name = "Primary Contact: Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address for Primary Contact.")]
        public string PrimaryContactEmail { get; set; }
        [Display(Name = "Primary Contact: Phone")]
        [Phone(ErrorMessage = "Invalid Phone Number for Primary Contact.")]
        public string PrimaryContactPhone { get; set; }
    }
}