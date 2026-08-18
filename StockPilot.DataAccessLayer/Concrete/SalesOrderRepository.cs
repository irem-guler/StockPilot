using Microsoft.EntityFrameworkCore;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class SalesOrderRepository
        : GenericRepository<SalesOrder>, ISalesOrderDal
    {
        public SalesOrderRepository(StockPilotContext context)
            : base(context)
        {
        }

        public async Task<List<SalesOrder>> GetAllWithDetailsAsync()
        {
            return await _context.SalesOrders
                .Include(order => order.Customer)
                .Include(order => order.Warehouse)
                .Include(order => order.CreatedByUser)
                .Include(order => order.Items)
                    .ThenInclude(item => item.Product)
                .AsNoTracking()
                .OrderByDescending(order => order.OrderDateUtc)
                .ToListAsync();
        }

        public async Task<SalesOrder?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.SalesOrders
                .Include(order => order.Customer)
                .Include(order => order.Warehouse)
                .Include(order => order.CreatedByUser)
                .Include(order => order.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(order => order.SalesOrderId == id);
        }
    }
}