using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.BusinessLayer.Concrete
{
    public class PurchaseOrderManager : IPurchaseOrderService
    {
        private readonly IPurchaseOrderDal _purchaseOrderDal;
        private readonly IWarehouseStockDal _warehouseStockDal;
        private readonly IStockMovementDal _stockMovementDal;

        public PurchaseOrderManager(
            IPurchaseOrderDal purchaseOrderDal,
            IWarehouseStockDal warehouseStockDal,
            IStockMovementDal stockMovementDal)
        {
            _purchaseOrderDal = purchaseOrderDal;
            _warehouseStockDal = warehouseStockDal;
            _stockMovementDal = stockMovementDal;
        }

        public async Task<List<PurchaseOrder>> GetAllAsync()
        {
            return await _purchaseOrderDal.GetAllWithDetailsAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _purchaseOrderDal.GetByIdWithDetailsAsync(id);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(
            PurchaseOrder order)
        {
            if (order.SupplierId <= 0)
            {
                return (false, "Please select a supplier.");
            }

            if (order.WarehouseId <= 0)
            {
                return (false, "Please select a destination warehouse.");
            }

            var validItems = order.Items
                .Where(item => item.ProductId > 0 && item.Quantity > 0)
                .ToList();

            if (validItems.Count == 0)
            {
                return (false, "The order must contain at least one valid item.");
            }

            foreach (var item in validItems)
            {
                if (item.UnitPrice < 0)
                {
                    return (false, "Unit price cannot be negative.");
                }
            }

            order.Items = validItems;
            order.Status = PurchaseOrderStatus.Pending;
            order.OrderDateUtc = DateTime.UtcNow;
            order.ReceivedDateUtc = null;

            await _purchaseOrderDal.AddAsync(order);
            await _purchaseOrderDal.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> CancelAsync(int id)
        {
            var order = await _purchaseOrderDal.GetByIdWithDetailsAsync(id);

            if (order == null)
            {
                return (false, "Order not found.");
            }

            if (order.Status == PurchaseOrderStatus.Received)
            {
                return (false, "A received order cannot be cancelled.");
            }

            if (order.Status == PurchaseOrderStatus.Cancelled)
            {
                return (false, "This order is already cancelled.");
            }

            order.Status = PurchaseOrderStatus.Cancelled;

            _purchaseOrderDal.Update(order);
            await _purchaseOrderDal.SaveChangesAsync();

            return (true, null);
        }
        public async Task<(bool Success, string? ErrorMessage)> ReceiveAsync(
    int id, string? performedByUserId)
        {
            var order = await _purchaseOrderDal.GetByIdWithDetailsAsync(id);

            if (order == null)
            {
                return (false, "Order not found.");
            }

            if (order.Status == PurchaseOrderStatus.Received)
            {
                return (false, "This order has already been received.");
            }

            if (order.Status == PurchaseOrderStatus.Cancelled)
            {
                return (false, "A cancelled order cannot be received.");
            }

            if (order.Items == null || order.Items.Count == 0)
            {
                return (false, "The order has no items to receive.");
            }

            await _stockMovementDal.BeginTransactionAsync();

            try
            {
                foreach (var item in order.Items)
                {
                    var warehouseStock = await _warehouseStockDal
                        .GetByProductAndWarehouseAsync(item.ProductId, order.WarehouseId);

                    if (warehouseStock == null)
                    {
                        warehouseStock = new WarehouseStock
                        {
                            ProductId = item.ProductId,
                            WarehouseId = order.WarehouseId,
                            Quantity = item.Quantity
                        };

                        await _warehouseStockDal.AddAsync(warehouseStock);
                    }
                    else
                    {
                        warehouseStock.Quantity += item.Quantity;
                        _warehouseStockDal.Update(warehouseStock);
                    }

                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        SourceWarehouseId = null,
                        DestinationWarehouseId = order.WarehouseId,
                        MovementType = StockMovementType.StockIn,
                        Quantity = item.Quantity,
                        Description = $"Purchase Order #{order.PurchaseOrderId} received",
                        PerformedByUserId = performedByUserId
                    };

                    await _stockMovementDal.AddAsync(movement);
                }

                order.Status = PurchaseOrderStatus.Received;
                order.ReceivedDateUtc = DateTime.UtcNow;
                _purchaseOrderDal.Update(order);

                await _warehouseStockDal.SaveChangesAsync();
                await _stockMovementDal.CommitTransactionAsync();

                return (true, null);
            }
            catch
            {
                await _stockMovementDal.RollbackTransactionAsync();
                return (false, "An error occurred while receiving the order. The operation was cancelled.");
            }
        }
    }
}