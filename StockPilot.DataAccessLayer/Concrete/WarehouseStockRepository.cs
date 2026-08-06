using Microsoft.EntityFrameworkCore;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class WarehouseStockRepository
        : GenericRepository<WarehouseStock>, IWarehouseStockDal
    {
        public WarehouseStockRepository(StockPilotContext context)
            : base(context)
        {
        }

        public async Task<WarehouseStock?> GetByProductAndWarehouseAsync(
            int productId,
            int warehouseId)
        {
            return await _context.WarehouseStocks
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId);
        }

        public async Task<List<WarehouseStock>> GetInventoryAsync(
            string? searchTerm,
            int? warehouseId)
        {
            var query = _context.WarehouseStocks
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Where(x =>
                    x.Product.IsActive &&
                    x.Warehouse.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(x =>
                    x.Product.Name.Contains(searchTerm) ||
                    x.Product.SKU.Contains(searchTerm));
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(x =>
                    x.WarehouseId == warehouseId.Value);
            }

            return await query
                .OrderBy(x => x.Product.Name)
                .ThenBy(x => x.Warehouse.Name)
                .ToListAsync();
        }
    }
}