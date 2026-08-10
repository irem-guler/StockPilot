using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;
using StockPilot.EntityLayer.Entities;
using Microsoft.EntityFrameworkCore;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class StockMovementRepository
        : GenericRepository<StockMovement>, IStockMovementDal
    {
        private IDbContextTransaction? _currentTransaction;

        public StockMovementRepository(StockPilotContext context)
            : base(context)
        {
        }

        public async Task BeginTransactionAsync()
        {
            _currentTransaction =
                await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task<List<StockMovement>> GetMovementsAsync(
    int? productId,
    int? warehouseId,
    StockMovementType? movementType)
        {
            var query = _dbSet
                .Include(movement => movement.Product)
                .Include(movement => movement.SourceWarehouse)
                .Include(movement => movement.DestinationWarehouse)
                .Include(movement => movement.PerformedByUser)
                .AsNoTracking()
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(movement =>
                    movement.ProductId == productId.Value);
            }

            if (warehouseId.HasValue)
            {
                query = query.Where(movement =>
                    movement.SourceWarehouseId == warehouseId.Value ||
                    movement.DestinationWarehouseId == warehouseId.Value);
            }

            if (movementType.HasValue)
            {
                query = query.Where(movement =>
                    movement.MovementType == movementType.Value);
            }

            return await query
                .OrderByDescending(movement => movement.MovementDateUtc)
                .ToListAsync();
        }
    }
}