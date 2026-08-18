using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Context
{
    public class StockPilotContext : IdentityDbContext<AppUser>
    {
        public StockPilotContext(DbContextOptions<StockPilotContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }

        public DbSet<WarehouseStock> WarehouseStocks { get; set; }

        public DbSet<StockMovement> StockMovements { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }

        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
    .Property(x => x.UnitPrice)
    .HasPrecision(18, 2);

            modelBuilder.Entity<WarehouseStock>()
                .HasIndex(x => new { x.ProductId, x.WarehouseId })
                .IsUnique();
            modelBuilder.Entity<Product>()
    .HasIndex(x => x.SKU)
    .IsUnique();
            modelBuilder.Entity<StockMovement>()
                .HasOne(x => x.SourceWarehouse)
                .WithMany(x => x.OutgoingStockMovements)
                .HasForeignKey(x => x.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(x => x.DestinationWarehouse)
                .WithMany(x => x.IncomingStockMovements)
                .HasForeignKey(x => x.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(x => x.PerformedByUser)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.PerformedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PurchaseOrder>()
    .HasOne(x => x.Supplier)
    .WithMany()
    .HasForeignKey(x => x.SupplierId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<SalesOrder>()
    .HasOne(x => x.Customer)
    .WithMany()
    .HasForeignKey(x => x.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrder>()
                .HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(x => x.SalesOrder)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesOrderItem>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesOrderItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);
        }
    }
}