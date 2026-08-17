using Microsoft.EntityFrameworkCore;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class PurchaseOrderRepository
        : GenericRepository<PurchaseOrder>, IPurchaseOrderDal
    {
        public PurchaseOrderRepository(StockPilotContext context)
            : base(context)
        {
        }

        public async Task<List<PurchaseOrder>> GetAllWithDetailsAsync()
        {
            return await _context.PurchaseOrders
                .Include(order => order.Supplier)
                .Include(order => order.Warehouse)
                .Include(order => order.CreatedByUser)
                .Include(order => order.Items)
                    .ThenInclude(item => item.Product)
                .AsNoTracking()
                .OrderByDescending(order => order.OrderDateUtc)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(order => order.Supplier)
                .Include(order => order.Warehouse)
                .Include(order => order.CreatedByUser)
                .Include(order => order.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(order => order.PurchaseOrderId == id);
        }
    }
}