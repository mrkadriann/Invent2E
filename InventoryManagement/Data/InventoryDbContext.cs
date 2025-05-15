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
        }
    }
}
