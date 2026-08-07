using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Abstract
{
    public interface IInventoryService
    {
        Task<List<WarehouseStock>> GetInventoryAsync(
            string? searchTerm,
            int? warehouseId);

        Task<WarehouseStock?> GetByProductAndWarehouseAsync(
            int productId,
            int warehouseId);

        Task<(bool Success, string? ErrorMessage)> StockInAsync(
    int productId,
    int warehouseId,
    int quantity,
    string? note);

        Task<(bool Success, string? ErrorMessage)> StockOutAsync(
    int productId,
    int warehouseId,
    int quantity,
    string? note);

        Task<(bool Success, string? ErrorMessage)> TransferAsync(
    int productId,
    int sourceWarehouseId,
    int destinationWarehouseId,
    int quantity,
    string? note);
    }

}