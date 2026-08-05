using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class ProductRepository
        : GenericRepository<Product>, IProductDal
    {
        public ProductRepository(StockPilotContext context)
            : base(context)
        {
        }
    }
}