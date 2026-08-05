using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class StockMovementRepository
        : GenericRepository<StockMovement>, IStockMovementDal
    {
        public StockMovementRepository(StockPilotContext context)
            : base(context)
        {
        }
    }
}
