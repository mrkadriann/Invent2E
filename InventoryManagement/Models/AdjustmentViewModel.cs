// ~/Models/AdjustmentViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace InventoryManagement.Models
{
    public class AdjustmentViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Current Stock")]
        public int CurrentStockQuantity { get; set; }

        public string StockStatus { get; set; } // e.g., "Low", "Normal"

        public string ProductImageUrl { get; set; } = "/images/placeholder-sm.jpg"; // Default/placeholder image

        [Required(ErrorMessage = "Adjustment type is required.")]
        [Display(Name = "Adjustment Type")]
        public string AdjustmentTypeValue { get; set; } = "Quantity"; // Default to Quantity

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; }

        [Display(Name = "New Quantity on Hand")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        // Required only if AdjustmentTypeValue is "Quantity" - handled by controller or client-side validation
        public int? AdjustedQuantity { get; set; }

        // For Value Adjustment (can be expanded later)
        [Display(Name = "Adjustment Value")]
        [DataType(DataType.Currency)]
        public decimal? AdjustmentValueAmount { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        public List<SelectListItem> AdjustmentReasons { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select Reason..." },
            new SelectListItem { Value = "StockCount", Text = "Stock Count Correction" },
            new SelectListItem { Value = "DamagedGoods", Text = "Damaged Goods" },
            new SelectListItem { Value = "StolenGoods", Text = "Stolen Goods" },
            new SelectListItem { Value = "LostGoods", Text = "Lost Goods" },
            new SelectListItem { Value = "FoundGoods", Text = "Found Goods / Inbound Error" },
            new SelectListItem { Value = "Return", Text = "Customer Return (Restock)" },
            new SelectListItem { Value = "Promotion", Text = "Promotional Use / Giveaway" },
            new SelectListItem { Value = "InternalUse", Text = "Internal Use / Samples" },
            new SelectListItem { Value = "Obsolete", Text = "Obsolete / Expired Stock" },
            new SelectListItem { Value = "Other", Text = "Other (Specify in Description)" }
        };
    }
}