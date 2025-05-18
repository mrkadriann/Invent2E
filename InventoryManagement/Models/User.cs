using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [NotMapped]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        public string CompanyName { get; set; }

        [Required]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "Phone number must be 11 digits and start with 09.")]
        public string Phone { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "You must agree to the terms and conditions.")]
        public bool Terms { get; set; }



        public string? VerificationCode { get; set; }

        public bool IsVerified { get; set; } = false;



    }
}
