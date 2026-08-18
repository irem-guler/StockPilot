using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Abstract
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrder>> GetAllAsync();

        Task<PurchaseOrder?> GetByIdAsync(int id);

        Task<(bool Success, string? ErrorMessage)> CreateAsync(PurchaseOrder order);

        Task<(bool Success, string? ErrorMessage)> CancelAsync(int id);
        Task<(bool Success, string? ErrorMessage)> ReceiveAsync(int id, string? performedByUserId);
    }
}