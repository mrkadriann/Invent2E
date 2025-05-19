using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Represents an order in the system.
    /// Integration Note: This model is used by the Orders module and expects data from:
    /// - Customers module (CustomerID)
    /// - Products module (via OrderItems)
    /// - Payment module (PaymentMethod, PaymentDate)
    /// </summary>
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public int CustomerID { get; set; } // Integration Point: Maps to Customers module's Customer ID

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } // Integration Point: Maps to Customers module's Customer Name

        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } // Integration Point: Should be populated from Customers module

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        [Required]
        public bool IsPaid { get; set; }

        [Required]
        public bool IsArchived { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Integration Point: Should be populated from Payment module

        public DateTime? PaymentDate { get; set; } // Integration Point: Should be populated from Payment module

        [Required]
        [StringLength(50)]
        public string ReferenceNumber { get; set; } // Integration Point: Should be populated from Payment module

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; }

        // New fields based on website functionality
        [Required]
        [StringLength(50)]
        public string OrderSource { get; set; } // Integration Point: Could be populated from multiple modules (e.g., "Website", "Mobile App", "POS")

        [Required]
        [StringLength(100)]
        public string Location { get; set; } // Integration Point: Could be populated from Inventory module

        [StringLength(50)]
        public string TrackingNumber { get; set; } // Integration Point: Should be populated from Shipping module

        [StringLength(50)]
        public string TrackingProvider { get; set; } // Integration Point: Should be populated from Shipping module

        public int ProgressPercentage { get; set; } // Calculated field based on Status

        // Property to hold selected product ID (for form binding)
        [NotMapped]
        public int? SelectedProductID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        public DateTime? ArchivedDate { get; set; }

        [StringLength(500)]
        public string? ArchiveReason { get; set; }

        public Order()
        {
            OrderItems = new List<OrderItem>();
            StatusHistory = new List<OrderStatusHistory>();
            OrderDate = DateTime.Now;
            Status = "New";
            IsPaid = false;
            IsArchived = false;
            OrderSource = "Website";
            Location = "Manila";
            ProgressPercentage = 0;
            ShippingAddress = "";
            PaymentMethod = "";
            ReferenceNumber = "";
            TrackingNumber = "";
            TrackingProvider = "";
            CustomerName = ""; // Initialize CustomerName
        }

        public ArchivedOrder ToArchivedOrder(string reason)
        {
            return new ArchivedOrder
            {
                OriginalOrderID = this.OrderID,
                OrderDate = this.OrderDate,
                CustomerID = this.CustomerID,
                ShippingAddress = this.ShippingAddress,
                OrderTotal = this.OrderTotal,
                IsPaid = this.IsPaid,
                PaymentMethod = this.PaymentMethod,
                PaymentDate = this.PaymentDate,
                ReferenceNumber = this.ReferenceNumber,
                Status = this.Status,
                OrderSource = this.OrderSource,
                Location = this.Location,
                TrackingNumber = this.TrackingNumber,
                TrackingProvider = this.TrackingProvider,
                ProgressPercentage = this.ProgressPercentage,
                PaymentAmount = this.PaymentAmount,
                ArchivedDate = DateTime.Now,
                ArchiveReason = reason
            };
        }
    }

    /// <summary>
    /// Represents an archived order in the system.
    /// This model stores historical order data that has been archived.
    /// </summary>
    public class ArchivedOrder
    {
        [Key]
        public int ArchivedOrderID { get; set; }

        [Required]
        public int OriginalOrderID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public int CustomerID { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        [Required]
        public bool IsPaid { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string ReferenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderSource { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(50)]
        public string TrackingNumber { get; set; }

        [StringLength(50)]
        public string TrackingProvider { get; set; }

        public int ProgressPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime ArchivedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ArchivedBy { get; set; }

        [StringLength(500)]
        public string? ArchiveReason { get; set; }

        // Navigation properties for archived items
        public virtual ICollection<ArchivedOrderItem> ArchivedOrderItems { get; set; }

        public ArchivedOrder()
        {
            ArchivedOrderItems = new List<ArchivedOrderItem>();
            ArchivedDate = DateTime.Now;
            ShippingAddress = string.Empty;
            PaymentMethod = string.Empty;
            ReferenceNumber = string.Empty;
            Status = string.Empty;
            OrderSource = string.Empty;
            Location = string.Empty;
            TrackingNumber = string.Empty;
            TrackingProvider = string.Empty;
            ArchiveReason = string.Empty;
        }
    }

    /// <summary>
    /// Represents an item within an archived order.
    /// </summary>
    public class ArchivedOrderItem
    {
        [Key]
        public int ArchivedOrderItemID { get; set; }

        [Required]
        public int ArchivedOrderID { get; set; }

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

        // Navigation property
        public virtual ArchivedOrder? ArchivedOrder { get; set; }

        public ArchivedOrderItem()
        {
            ProductName = string.Empty;
            Quantity = 1;
            UnitPrice = 0;
            TotalPrice = 0;
        }
    }

    /// <summary>
    /// Tracks the history of status changes for an order.
    /// Integration Note: This model is used internally by the Orders module but can be
    /// populated by other modules when they change the order status.
    /// </summary>
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderID { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [Required]
        public DateTime StatusDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation property
        public virtual Order? Order { get; set; }

        public OrderStatusHistory()
        {
            StatusDate = DateTime.Now;
            Status = string.Empty;
            Notes = string.Empty;
        }
    }

    /// <summary>
    /// Tracks the history of archiving operations for orders.
    /// </summary>
    public class ArchiveHistory
    {
        [Key]
        public int ArchiveHistoryID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [Required]
        public int ArchivedOrderID { get; set; }

        [Required]
        public DateTime ArchiveDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ArchivedBy { get; set; }

        [Required]
        [StringLength(500)]
        public string ArchiveReason { get; set; }

        [StringLength(50)]
        public string? PreviousStatus { get; set; }

        [StringLength(50)]
        public string? PreviousLocation { get; set; }

        // Navigation properties
        public virtual Order? Order { get; set; }
        public virtual ArchivedOrder? ArchivedOrder { get; set; }

        public ArchiveHistory()
        {
            ArchiveDate = DateTime.Now;
            ArchivedBy = "System";
            ArchiveReason = string.Empty;
        }
    }
}