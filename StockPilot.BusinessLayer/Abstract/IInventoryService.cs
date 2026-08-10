using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

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
            string? note,
            string? performedByUserId);

        Task<(bool Success, string? ErrorMessage)> StockOutAsync(
            int productId,
            int warehouseId,
            int quantity,
            string? note,
            string? performedByUserId);

        Task<(bool Success, string? ErrorMessage)> TransferAsync(
            int productId,
            int sourceWarehouseId,
            int destinationWarehouseId,
            int quantity,
            string? note,
            string? performedByUserId);

        Task<List<StockMovement>> GetMovementsAsync(
            int? productId,
            int? warehouseId,
            StockMovementType? movementType);
    }
}