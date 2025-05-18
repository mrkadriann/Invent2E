using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    [Table("customer")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("customer_picture")]
        public byte[]? CustomerPicture { get; set; }

        [Required]
        [Column("customer_name")]
        public string CustomerName { get; set; }

        [Required]
        [Column("customer_type")]
        public string CustomerType { get; set; }
        [Required]
        [Column("customer_status")]
        public string CustomerStatus { get; set; }

        [Column("customer_remark")]
        public string? CustomerRemark { get; set; }
        public virtual CustomerInformation Information { get; set; }
    }

    [Table("customer_information")]
    public class CustomerInformation
    {
        [Key]
        [Column("customer_info_id")]
        public int CustomerInfoId { get; set; }

        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Required]
        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Column("customer_email")]
        public string CustomerEmail { get; set; }

        [Required]
        [Column("shipping_address")]
        public string ShippingAddress { get; set; }

        [Required]
        [Column("billing_address")]
        public string BillingAddress { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
