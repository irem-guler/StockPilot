using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class CustomerRepository
        : GenericRepository<Customer>, ICustomerDal
    {
        public CustomerRepository(StockPilotContext context)
            : base(context)
        {
        }
    }
}