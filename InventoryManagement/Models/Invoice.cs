using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual ICollection<InvoiceItem> Items { get; set; }

        public Invoice()
        {
            InvoiceDate = DateTime.Now;
            Status = "Pending";
            Items = new List<InvoiceItem>();
            InvoiceNumber = GenerateInvoiceNumber();
            PaymentMethod = string.Empty;
            ShippingAddress = string.Empty;
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
    }

    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemID { get; set; }

        [Required]
        public int InvoiceID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        public virtual Invoice? Invoice { get; set; }
        public virtual Product? Product { get; set; }

        public InvoiceItem()
        {
            ProductName = string.Empty;
            Quantity = 1;
            UnitPrice = 0;
            TotalPrice = 0;
        }
    }
} 