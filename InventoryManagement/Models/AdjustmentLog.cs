// ~/Models/AdjustmentLog.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class AdjustmentLog
    {
        [Key]
        [Column("LogId")]
        public int LogId { get; set; }

        [Required]
        [Column("ProductId")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        [Column("AdjustmentDate")]
        public DateTime AdjustmentDate { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Reason")]
        public string Reason { get; set; }

        [MaxLength(500)]
        [Column("Description")]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("AdjustmentType")]
        public string AdjustmentType { get; set; }

        [Column("OldQuantity")]
        public int? OldQuantity { get; set; }

        [Column("NewQuantity")]
        public int? NewQuantity { get; set; }

        [Column("OldValue", TypeName = "decimal(18, 2)")]
        public decimal? OldValue { get; set; }

        [Column("NewValue", TypeName = "decimal(18, 2)")]
        public decimal? NewValue { get; set; }

        [MaxLength(256)]
        [Column("UserId")]
        public string UserId { get; set; }

        [Required]
        [Column("LogTimestamp")]
        public DateTime LogTimestamp { get; set; } = DateTime.UtcNow;
    }
}