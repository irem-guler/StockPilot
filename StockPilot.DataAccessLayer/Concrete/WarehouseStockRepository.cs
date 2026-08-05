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

        public async Task<WarehouseStock?>
            GetByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            return await _dbSet.FirstOrDefaultAsync(x =>
                x.ProductId == productId &&
                x.WarehouseId == warehouseId);
        }
    }
}