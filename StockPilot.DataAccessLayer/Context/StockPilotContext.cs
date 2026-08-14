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
        }
    }
}