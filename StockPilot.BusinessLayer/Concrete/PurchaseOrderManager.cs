using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.BusinessLayer.Concrete
{
    public class PurchaseOrderManager : IPurchaseOrderService
    {
        private readonly IPurchaseOrderDal _purchaseOrderDal;

        public PurchaseOrderManager(IPurchaseOrderDal purchaseOrderDal)
        {
            _purchaseOrderDal = purchaseOrderDal;
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
    }
}