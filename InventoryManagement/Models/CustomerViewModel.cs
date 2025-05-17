using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Models
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? CustomerPictureFile { get; set; }
        public bool HasExistingPicture { get; set; } = false;

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(255)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Customer type is required.")]
        [StringLength(50)]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Required(ErrorMessage = "Customer status is required.")]
        [StringLength(50)]
        [Display(Name = "Customer Status")]
        public string CustomerStatus { get; set; }

        [Display(Name = "Remarks")]
        public string? CustomerRemark { get; set; }

        public int CustomerInfoId { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(255)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Shipping address is required.")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Billing address is required.")]
        [Display(Name = "Billing Address")]
        public string BillingAddress { get; set; }
    }
}