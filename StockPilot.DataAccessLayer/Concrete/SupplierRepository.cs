using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class SupplierRepository
        : GenericRepository<Supplier>, ISupplierDal
    {
        public SupplierRepository(StockPilotContext context)
            : base(context)
        {
        }
    }
}