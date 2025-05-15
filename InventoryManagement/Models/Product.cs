using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; } // Primary Key
        [Required]
        [Column("item_name")]
        public string ItemName { get; set; }
        [Required]
        [Column("supplier")]
        public string Supplier {  get; set; } // Pwede siyang gawing SupplierId when collaborating
        [Column("category_id")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [Column("image_id")]
        public int? PrimaryImageId { get; set; }
   
        [ForeignKey("PrimaryImageId")]
        public virtual ImageData PrimaryImage { get; set; }

        public virtual ICollection<ImageData> AllImages { get; set; }
        public virtual Description Description { get; set; }
        public virtual Quantity Quantity { get; set; }
        // stock status is 30 - Low, 50 - Normal, 100 - Overstocked, 20 - Danger

        public Product()
        {
            AllImages = new HashSet<ImageData>();
        }
    }

    [Table("category")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("category_name")]
        public string CategoryName { get; set; }

        public virtual ICollection<Product> Products { get; set; } 
    }

    [Table("descriptions")]
    public class Description
    {
        [Key]
        [Column("description_id")]
        public int DescriptionId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Required]
        [Column("description")]
        public string DescriptionText { get; set; }
        [Required]
        [Column("height")]
        public string Height { get; set; }
        [Required]
        [Column("width")]
        public string Width { get; set; }
        [Required]
        [Column("color")]
        public string Color { get; set; }
        [Column("wholesale_price")]
        public decimal WholesalePrice { get; set; }
        [Column("retail_price")]
        public decimal RetailPrice { get; set; }
        [Column("profit")]
        public decimal Profit { get; set; }

        [ForeignKey("ItemId")]
        public virtual Product Product { get; set; }
    }

    [Table("image")]
    public class ImageData
    {
        [Key]
        [Column("image_id")]
        public int ImageId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Required]
        [Column("image_data")]
        public byte[] ImageDt { get; set; }
        [Column("image_order")]
        public byte? ImageOrder { get; set; }

        [ForeignKey("ItemId")]
        public virtual Product Product { get; set; }

        public virtual Product ProductAsPrimary {  get; set; }
    }

    [Table("quantity")]
    public class Quantity
    {
        [Key]
        [Column("quantity_id")]
        public int QuantityId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Column("quantity")]
        public int Qty {  get; set; }

        [ForeignKey("ItemId")]
        public virtual Product Product { get; set; }
    }
}
