using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Models
{
    public class SupplierContactViewModel
    {
        public int ContactId { get; set; }

        [Display(Name = "Contact Name")]
        public string Name { get; set; }

        [Display(Name = "Contact Email")]
        public string Email { get; set; }
        public string Phone { get; set; }
    }
    public class AddSupplierViewModel
    {

        public int SupplierId { get; set; }

        public bool HasExistingProfileImage { get; set; }

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

        [Display(Name = "Primary Contact: Name")]
        public string? PrimaryContactName { get; set; } // Added ?

        [Display(Name = "Primary Contact: Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address for Primary Contact.")]
        public string? PrimaryContactEmail { get; set; } // Added ?

        [Display(Name = "Primary Contact: Phone")]
        [Phone(ErrorMessage = "Invalid Phone Number for Primary Contact.")]
        public string? PrimaryContactPhone { get; set; } // Added ?
        public List<SupplierContactViewModel> OtherContacts { get; set; } = new List<SupplierContactViewModel>();
    }
}