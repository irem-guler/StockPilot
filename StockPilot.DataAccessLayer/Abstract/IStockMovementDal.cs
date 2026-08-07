using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Abstract
{
    public interface IStockMovementDal : IGenericDal<StockMovement>
    {
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}