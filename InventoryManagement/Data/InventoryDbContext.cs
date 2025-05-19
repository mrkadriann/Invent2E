using InventoryManagement.Models; // To access your entity classes
using Microsoft.EntityFrameworkCore;
using TransactionF.Models;

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

        //Supplier
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierContact> SupplierContacts { get; set; }

        //Customer
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerInformation> CustomerInformations { get; set; }

        //Transaction
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<ArchivedOrder> ArchivedOrders { get; set; }
        public DbSet<ArchivedOrderItem> ArchivedOrderItems { get; set; }
        public DbSet<ArchiveHistory> ArchiveHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product", "dbo");
                entity.HasKey(e => e.ItemId);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SupplierId).IsRequired();
                entity.Property(e => e.CategoryId);
                entity.Property(e => e.PrimaryImageId);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PrimaryImage)
                    .WithOne(i => i.ProductAsPrimary)
                    .HasForeignKey<Product>(p => p.PrimaryImageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.AllImages)
                    .WithOne(i => i.Product)
                    .HasForeignKey(i => i.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Description)
                    .WithOne(d => d.Product)
                    .HasForeignKey<Description>(d => d.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Quantity)
                    .WithOne(q => q.Product)
                    .HasForeignKey<Quantity>(q => q.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add index for item ID
                entity.HasIndex(e => e.ItemId);
            });

            modelBuilder.Entity<Quantity>()
                .Property(q => q.Qty)
                .HasDefaultValue(0);

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

            //Transaction
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems", "dbo");
                entity.HasKey(e => e.OrderItemID);
                entity.Property(e => e.ItemId).HasColumnName("ProductID").IsRequired();
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderID)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders", "dbo");
                entity.HasKey(e => e.OrderID);
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.CustomerID).IsRequired();
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OrderTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.OrderSource).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.TrackingNumber).HasMaxLength(50);
                entity.Property(e => e.TrackingProvider).HasMaxLength(50);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
                entity.Property(e => e.IsPaid).IsRequired();
                entity.Property(e => e.IsArchived).IsRequired();
                entity.Property(e => e.ProgressPercentage).HasColumnType("decimal(5,2)");

                // Add indexes for frequently queried fields
                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsArchived);
                entity.HasIndex(e => e.CustomerID);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices", "dbo");
                entity.HasKey(e => e.InvoiceID);
                entity.Property(e => e.OrderID).IsRequired();
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InvoiceDate).IsRequired();
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentDate).IsRequired();
                entity.Property(e => e.ReferenceNumber).HasMaxLength(50);

                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.ToTable("InvoiceItems", "dbo");
                entity.HasKey(e => e.InvoiceItemID);
                entity.Property(e => e.InvoiceID).IsRequired();
                entity.Property(e => e.ProductID).IsRequired();
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Invoice)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.InvoiceID)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ArchivedOrder>(entity =>
            {
                entity.ToTable("ArchivedOrders", "dbo");
                entity.HasKey(e => e.ArchivedOrderID);
                entity.Property(e => e.OriginalOrderID).IsRequired();
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.CustomerID).IsRequired();
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OrderTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.OrderSource).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.TrackingNumber).HasMaxLength(50);
                entity.Property(e => e.TrackingProvider).HasMaxLength(50);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
                entity.Property(e => e.ArchivedDate).IsRequired();
                entity.Property(e => e.ArchivedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ArchiveReason).IsRequired().HasMaxLength(500);

                // Add indexes for archived orders
                entity.HasIndex(e => e.ArchivedDate);
                entity.HasIndex(e => e.OriginalOrderID);
            });

            modelBuilder.Entity<ArchivedOrderItem>(entity =>
            {
                entity.ToTable("ArchivedOrderItems", "dbo");
                entity.HasKey(e => e.ArchivedOrderItemID);
                entity.Property(e => e.ArchivedOrderID).IsRequired();
                entity.Property(e => e.ProductID).IsRequired();
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ArchivedOrder)
                    .WithMany(o => o.ArchivedOrderItems)
                    .HasForeignKey(e => e.ArchivedOrderID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ArchiveHistory>(entity =>
            {
                entity.ToTable("ArchiveHistories", "dbo");
                entity.HasKey(e => e.ArchiveHistoryID);
                entity.Property(e => e.OrderID).IsRequired();
                entity.Property(e => e.ArchivedOrderID).IsRequired();
                entity.Property(e => e.ArchiveDate).IsRequired();
                entity.Property(e => e.ArchivedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ArchiveReason).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PreviousStatus).HasMaxLength(50);
                entity.Property(e => e.PreviousLocation).HasMaxLength(50);

                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ArchivedOrder)
                    .WithMany()
                    .HasForeignKey(e => e.ArchivedOrderID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add index for archive date
                entity.HasIndex(e => e.ArchiveDate);
            });
        }
    }
}
