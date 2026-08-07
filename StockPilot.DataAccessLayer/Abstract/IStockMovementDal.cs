using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.DataAccessLayer.Abstract
{
    public interface IStockMovementDal : IGenericDal<StockMovement>
    {
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        Task<List<StockMovement>> GetMovementsAsync(
            int? productId,
            int? warehouseId,
            StockMovementType? movementType);
    }
}