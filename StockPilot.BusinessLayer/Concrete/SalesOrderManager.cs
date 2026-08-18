using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.BusinessLayer.Concrete
{
    public class SalesOrderManager : ISalesOrderService
    {
        private readonly ISalesOrderDal _salesOrderDal;
        private readonly IWarehouseStockDal _warehouseStockDal;
        private readonly IStockMovementDal _stockMovementDal;

        public SalesOrderManager(
            ISalesOrderDal salesOrderDal,
            IWarehouseStockDal warehouseStockDal,
            IStockMovementDal stockMovementDal)
        {
            _salesOrderDal = salesOrderDal;
            _warehouseStockDal = warehouseStockDal;
            _stockMovementDal = stockMovementDal;
        }

        public async Task<List<SalesOrder>> GetAllAsync()
        {
            return await _salesOrderDal.GetAllWithDetailsAsync();
        }

        public async Task<SalesOrder?> GetByIdAsync(int id)
        {
            return await _salesOrderDal.GetByIdWithDetailsAsync(id);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(
            SalesOrder order)
        {
            if (order.CustomerId <= 0)
            {
                return (false, "Please select a customer.");
            }

            if (order.WarehouseId <= 0)
            {
                return (false, "Please select a source warehouse.");
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
            order.Status = SalesOrderStatus.Pending;
            order.OrderDateUtc = DateTime.UtcNow;
            order.ShippedDateUtc = null;

            await _salesOrderDal.AddAsync(order);
            await _salesOrderDal.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> CancelAsync(int id)
        {
            var order = await _salesOrderDal.GetByIdWithDetailsAsync(id);

            if (order == null)
            {
                return (false, "Order not found.");
            }

            if (order.Status == SalesOrderStatus.Shipped)
            {
                return (false, "A shipped order cannot be cancelled.");
            }

            if (order.Status == SalesOrderStatus.Cancelled)
            {
                return (false, "This order is already cancelled.");
            }

            order.Status = SalesOrderStatus.Cancelled;

            _salesOrderDal.Update(order);
            await _salesOrderDal.SaveChangesAsync();

            return (true, null);
        }
        public async Task<(bool Success, string? ErrorMessage)> ShipAsync(
    int id, string? performedByUserId)
        {
            var order = await _salesOrderDal.GetByIdWithDetailsAsync(id);

            if (order == null)
            {
                return (false, "Order not found.");
            }

            if (order.Status == SalesOrderStatus.Shipped)
            {
                return (false, "This order has already been shipped.");
            }

            if (order.Status == SalesOrderStatus.Cancelled)
            {
                return (false, "A cancelled order cannot be shipped.");
            }

            if (order.Items == null || order.Items.Count == 0)
            {
                return (false, "The order has no items to ship.");
            }

            // Önce tüm kalemler için yeterli stok var mı kontrol et
            foreach (var item in order.Items)
            {
                var stock = await _warehouseStockDal
                    .GetByProductAndWarehouseAsync(item.ProductId, order.WarehouseId);

                var available = stock?.Quantity ?? 0;

                if (available < item.Quantity)
                {
                    var productName = item.Product?.Name ?? $"Product #{item.ProductId}";

                    return (false,
                        $"Insufficient stock for '{productName}'. Required: {item.Quantity}, available: {available}.");
                }
            }

            await _stockMovementDal.BeginTransactionAsync();

            try
            {
                foreach (var item in order.Items)
                {
                    var stock = await _warehouseStockDal
                        .GetByProductAndWarehouseAsync(item.ProductId, order.WarehouseId);

                    stock!.Quantity -= item.Quantity;
                    _warehouseStockDal.Update(stock);

                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        SourceWarehouseId = order.WarehouseId,
                        DestinationWarehouseId = null,
                        MovementType = StockMovementType.StockOut,
                        Quantity = item.Quantity,
                        Description = $"Sales Order #{order.SalesOrderId} shipped",
                        PerformedByUserId = performedByUserId
                    };

                    await _stockMovementDal.AddAsync(movement);
                }

                order.Status = SalesOrderStatus.Shipped;
                order.ShippedDateUtc = DateTime.UtcNow;
                _salesOrderDal.Update(order);

                await _warehouseStockDal.SaveChangesAsync();
                await _stockMovementDal.CommitTransactionAsync();

                return (true, null);
            }
            catch
            {
                await _stockMovementDal.RollbackTransactionAsync();
                return (false, "An error occurred while shipping the order. The operation was cancelled.");
            }
        }
    }
}