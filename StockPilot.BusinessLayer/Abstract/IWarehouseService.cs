using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Abstract
{
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetAllAsync();

        Task<Warehouse?> GetByIdAsync(int id);

        Task AddAsync(Warehouse warehouse);

        Task UpdateAsync(Warehouse warehouse);
        Task<bool> DeactivateAsync(int id);
    }
}