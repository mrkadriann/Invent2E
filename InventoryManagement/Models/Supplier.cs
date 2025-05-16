using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    [Table("supplier")]
    public class Supplier
    {
        [Key]
        [Column("supplier_id")]
        public int SupplierId { get; set; }
        [Required]
        [Column("company_name")]
        public string CompanyName { get; set; }
        [Column("profile_image")]
        public byte[] ProfileImage { get; set; }
        [Column("person_name")]
        public string PersonName { get; set; }
        [Column("department")]
        public string Department {  get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Required]
        [Column("phone_number")]
        public string PhoneNumber { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("currency")]
        public string Currency {  get; set; }
        [Column("payment_method")]
        public string PaymentMethod { get; set; }
        [Column("courier")]
        public string Courier { get; set; }
        [Required]
        [Column("portal_status")]
        public string PortalStatus { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public virtual ICollection<SupplierContact> SupplierContacts { get; set; }

        public Supplier()
        {
            Products = new HashSet<Product>();
            SupplierContacts = new HashSet<SupplierContact>();
        }

    }

    [Table("supplierContact")]
    public class SupplierContact
    {
        [Key]
        [Column("contact_id")]
        public int ContactId {  get; set; }
        [Required]
        [Column("supplier_name")]
        public string ContactPersonName {  get; set; }
        [Required]
        [Column("email")]
        public string Email {  set; get; }
        [Required]
        [Column("phone_number")]
        public string PhoneNumber { get; set; }
        [Column("supplier_company_id")]
        public int SupplierCompanyId {  get; set; }

        [ForeignKey("SupplierCompanyId")]
        public virtual Supplier Supplier { get; set; }
    }
}
