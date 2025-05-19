using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Represents an item within an order.
    /// Integration Note: This model is used by the Orders module and expects data from the Products module.
    /// The Products module should provide ItemId, ProductName, and Size information.
    /// </summary>
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        [Column("ItemId")]
        public int ItemId { get; set; }

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
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }

        public OrderItem()
        {
            Quantity = 1;
            UnitPrice = 0;
            ProductName = string.Empty;
        }

        public OrderItem(int orderID, int itemId, int quantity, decimal unitPrice)
        {
            OrderID = orderID;
            ItemId = itemId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            ProductName = string.Empty;
        }
    }
}