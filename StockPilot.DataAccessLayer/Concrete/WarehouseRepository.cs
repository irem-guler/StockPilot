using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class WarehouseRepository
        : GenericRepository<Warehouse>, IWarehouseDal
    {
        public WarehouseRepository(StockPilotContext context)
            : base(context)
        {
        }
    }
}