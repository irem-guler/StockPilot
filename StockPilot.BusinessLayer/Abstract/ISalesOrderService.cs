using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Abstract
{
    public interface ISalesOrderService
    {
        Task<List<SalesOrder>> GetAllAsync();

        Task<SalesOrder?> GetByIdAsync(int id);

        Task<(bool Success, string? ErrorMessage)> CreateAsync(SalesOrder order);

        Task<(bool Success, string? ErrorMessage)> ShipAsync(int id, string? performedByUserId);

        Task<(bool Success, string? ErrorMessage)> CancelAsync(int id);
    }
}