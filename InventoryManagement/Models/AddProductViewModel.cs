// Models/AddProductViewModel.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class AddProductViewModel
    {
        [Required(ErrorMessage = "Product Name is required.")]
        [Display(Name = "Product Name")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Product Category is required.")]
        [Display(Name = "Product Category")]
        public int? SelectedProductCategoryId { get; set; }
        public List<SelectListItem>? ProductCategories { get; set; }

        [Display(Name = "Height")]
        public decimal? HeightValue { get; set; }
        public string HeightUnit { get; set; } = "cm";

        [Display(Name = "Width")]
        public decimal? WidthValue { get; set; }
        public string WidthUnit { get; set; } = "cm";
        public List<SelectListItem>? DimensionUnits { get; set; }

        [Display(Name = "Weight")]
        public decimal? WeightValue { get; set; }
        public string WeightUnit { get; set; } = "kg";
        public List<SelectListItem>? WeightUnits { get; set; }


        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Supplier")]
        public string? SelectedSupplierName { get; set; }
        public List<SelectListItem>? Suppliers { get; set; }

        [Display(Name = "Quantity Stocks")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
        public int QuantityInStock { get; set; }

        [Display(Name = "Product Description")]
        [DataType(DataType.MultilineText)]
        public string? ProductDescriptionText { get; set; }

        [Display(Name = "Wholesale Price")]
        [DataType(DataType.Currency)]
        public decimal? WholesalePrice { get; set; }

        [Display(Name = "Retail Price")]
        [DataType(DataType.Currency)]
        public decimal? RetailPrice { get; set; }


        [Display(Name = "Profit")]
        [DataType(DataType.Currency)]
        public decimal? Profit { get; set; }

        [Display(Name = "Add Images")]
        public List<IFormFile>? ImageFiles { get; set; }

        public AddProductViewModel()
        {
            ProductCategories = new List<SelectListItem>();
            Suppliers = new List<SelectListItem>();
            DimensionUnits = new List<SelectListItem>
            {
                new SelectListItem { Value = "cm", Text = "cm" },
                new SelectListItem { Value = "m", Text = "m" },
                new SelectListItem { Value = "in", Text = "in" },
                new SelectListItem { Value = "ft", Text = "ft" }
            };
            WeightUnits = new List<SelectListItem>
            {
                new SelectListItem { Value = "kg", Text = "kg" },
                new SelectListItem { Value = "g", Text = "g" },
                new SelectListItem { Value = "lb", Text = "lb" },
                new SelectListItem { Value = "oz", Text = "oz" }
            };
            ImageFiles = new List<IFormFile>();
        }
    }
}