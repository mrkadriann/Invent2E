using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class EditProductViewModel
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [Display(Name = "Product Name")]
        public string ItemName { get; set; }

        [Display(Name = "Product Category")]
        public int? SelectedProductCategoryId { get; set; }
        public List<SelectListItem> ProductCategories { get; set; } = new List<SelectListItem>();

        [Display(Name = "Supplier")]
        public int? SelectedSupplierId { get; set; }
        public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();

        [Display(Name = "Product Description")]
        public string ProductDescriptionText { get; set; }

        [Display(Name = "Quantity In Stock")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int QuantityInStock { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Height Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Height must be a positive number")]
        public double? HeightValue { get; set; }

        [Display(Name = "Height Unit")]
        public string HeightUnit { get; set; } = "cm";

        [Display(Name = "Width Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Width must be a positive number")]
        public double? WidthValue { get; set; }

        [Display(Name = "Width Unit")]
        public string WidthUnit { get; set; } = "cm";

        [Display(Name = "Weight Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Weight must be a positive number")]
        public double? WeightValue { get; set; }

        [Display(Name = "Weight Unit")]
        public string WeightUnit { get; set; } = "kg";

        [Display(Name = "Wholesale Price")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Wholesale price must be a positive number")]
        public decimal? WholesalePrice { get; set; }

        [Display(Name = "Retail Price")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Retail price must be a positive number")]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Profit")]
        [DataType(DataType.Currency)]
        public decimal? Profit { get; set; }

        [Display(Name = "Product Images")]
        public List<IFormFile>? NewImageFiles { get; set; }

        public List<ProductImageViewModel>? ExistingImages { get; set; } = new List<ProductImageViewModel>();
        public int? PrimaryImageId { get; set; }
        public string? DeletedImageIds { get; set; }
    }

    public class ProductImageViewModel
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public byte? ImageOrder { get; set; }
        public bool? IsPrimary
        {
            get; set;
        }
    }
}