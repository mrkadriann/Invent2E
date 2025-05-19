using InventoryManagement.Models; // To access your entity classes
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; } 
        public DbSet<Quantity> Quantities { get; set; }
        public DbSet<ImageData> Images { get; set; }
        public DbSet<Description> Descriptions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AdjustmentLog> AdjustmentLogs { get; set; }

        //Supplier
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierContact> SupplierContacts { get; set; }

        //Customer
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerInformation> CustomerInformations { get; set; }

        //Settings
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.AllImages)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.PrimaryImage)
                .WithOne(i => i.ProductAsPrimary)
                .HasForeignKey<Product>(p => p.PrimaryImageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Description)
                .WithOne(d => d.Product)
                .HasForeignKey<Description>(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Quantity)
                .WithOne(q => q.Product)
                .HasForeignKey<Quantity>(q => q.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quantity>()
                .Property(q => q.Qty)
                .HasDefaultValue(0);

            modelBuilder.Entity<AdjustmentLog>()
               .HasOne(al => al.Product)          
               .WithMany()                       
               .HasForeignKey(al => al.ProductId) 
               .HasPrincipalKey(p => p.ItemId)
               .OnDelete(DeleteBehavior.Restrict);

            // Supplier
            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.SupplierContacts)
                .WithOne(sc => sc.Supplier)
                .HasForeignKey(sc => sc.SupplierCompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.CompanyName)
                .IsUnique();

            // Customer
            modelBuilder.Entity<Customer>()
               .HasOne(c => c.Information)
               .WithOne(ci => ci.Customer)
               .HasForeignKey<CustomerInformation>(ci => ci.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);


            // Settings
            modelBuilder.Entity<Settings>(entity =>
            {
                entity.Property(e => e.EnableTax).HasDefaultValue(false);
                entity.Property(e => e.NotifyLowStock).HasDefaultValue(false);
                entity.Property(e => e.NotifyOutOfStock).HasDefaultValue(false);
                entity.Property(e => e.NotifyReplenish).HasDefaultValue(false);
                entity.Property(e => e.DisableFeature1).HasDefaultValue(false);
                entity.Property(e => e.DisableFeature2).HasDefaultValue(false);
            });

        }
    }
}
