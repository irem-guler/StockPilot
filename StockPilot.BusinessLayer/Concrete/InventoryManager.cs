using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.BusinessLayer.Concrete
{
    public class InventoryManager : IInventoryService
    {
        private readonly IWarehouseStockDal _warehouseStockDal;
        private readonly IProductDal _productDal;
        private readonly IWarehouseDal _warehouseDal;
        private readonly IStockMovementDal _stockMovementDal;

        public InventoryManager(
            IWarehouseStockDal warehouseStockDal,
            IProductDal productDal,
            IWarehouseDal warehouseDal,
            IStockMovementDal stockMovementDal)
        {
            _warehouseStockDal = warehouseStockDal;
            _productDal = productDal;
            _warehouseDal = warehouseDal;
            _stockMovementDal = stockMovementDal;
        }
        public async Task<List<WarehouseStock>> GetInventoryAsync(
            string? searchTerm,
            int? warehouseId)
        {
            return await _warehouseStockDal.GetInventoryAsync(
                searchTerm,
                warehouseId);
        }

        public async Task<WarehouseStock?>
            GetByProductAndWarehouseAsync(
                int productId,
                int warehouseId)
        {
            return await _warehouseStockDal
                .GetByProductAndWarehouseAsync(
                    productId,
                    warehouseId);
        }
        public async Task<(bool Success, string? ErrorMessage)> StockInAsync(
    int productId,
    int warehouseId,
    int quantity,
    string? note)
        {
            if (quantity <= 0)
            {
                return (false, "Quantity must be greater than zero.");
            }

            var product = await _productDal.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
            {
                return (false, "Selected product was not found or is not active.");
            }

            var warehouse = await _warehouseDal.GetByIdAsync(warehouseId);

            if (warehouse == null || !warehouse.IsActive)
            {
                return (false, "Selected warehouse was not found or is not active.");
            }

            var warehouseStock = await _warehouseStockDal
                .GetByProductAndWarehouseAsync(productId, warehouseId);

            if (warehouseStock == null)
            {
                warehouseStock = new WarehouseStock
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = quantity
                };

                await _warehouseStockDal.AddAsync(warehouseStock);
            }
            else
            {
                warehouseStock.Quantity += quantity;

                _warehouseStockDal.Update(warehouseStock);
            }

            var stockMovement = new StockMovement
            {
                ProductId = productId,
                SourceWarehouseId = null,
                DestinationWarehouseId = warehouseId,
                MovementType = StockMovementType.StockIn,
                Quantity = quantity,
                Description = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                PerformedByUserId = null
            };

            await _stockMovementDal.AddAsync(stockMovement);

            await _warehouseStockDal.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> StockOutAsync(
    int productId,
    int warehouseId,
    int quantity,
    string? note)
        {
            if (quantity <= 0)
            {
                return (false, "Quantity must be greater than zero.");
            }

            var product = await _productDal.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
            {
                return (false, "Selected product was not found or is not active.");
            }

            var warehouse = await _warehouseDal.GetByIdAsync(warehouseId);

            if (warehouse == null || !warehouse.IsActive)
            {
                return (false, "Selected warehouse was not found or is not active.");
            }

            var warehouseStock = await _warehouseStockDal
                .GetByProductAndWarehouseAsync(productId, warehouseId);

            if (warehouseStock == null || warehouseStock.Quantity == 0)
            {
                return (false, "There is no stock for the selected product in this warehouse.");
            }

            if (warehouseStock.Quantity < quantity)
            {
                return (false, $"Insufficient stock. Available quantity is {warehouseStock.Quantity}.");
            }

            warehouseStock.Quantity -= quantity;

            _warehouseStockDal.Update(warehouseStock);

            var stockMovement = new StockMovement
            {
                ProductId = productId,
                SourceWarehouseId = warehouseId,
                DestinationWarehouseId = null,
                MovementType = StockMovementType.StockOut,
                Quantity = quantity,
                Description = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                PerformedByUserId = null
            };

            await _stockMovementDal.AddAsync(stockMovement);

            await _warehouseStockDal.SaveChangesAsync();

            return (true, null);
        }
        public async Task<(bool Success, string? ErrorMessage)> TransferAsync(
    int productId,
    int sourceWarehouseId,
    int destinationWarehouseId,
    int quantity,
    string? note)
        {
            if (quantity <= 0)
            {
                return (false, "Quantity must be greater than zero.");
            }

            if (sourceWarehouseId == destinationWarehouseId)
            {
                return (false, "Source and destination warehouses must be different.");
            }

            var product = await _productDal.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
            {
                return (false, "Selected product was not found or is not active.");
            }

            var sourceWarehouse = await _warehouseDal.GetByIdAsync(sourceWarehouseId);

            if (sourceWarehouse == null || !sourceWarehouse.IsActive)
            {
                return (false, "Source warehouse was not found or is not active.");
            }

            var destinationWarehouse =
                await _warehouseDal.GetByIdAsync(destinationWarehouseId);

            if (destinationWarehouse == null || !destinationWarehouse.IsActive)
            {
                return (false, "Destination warehouse was not found or is not active.");
            }

            var sourceStock = await _warehouseStockDal
                .GetByProductAndWarehouseAsync(productId, sourceWarehouseId);

            if (sourceStock == null || sourceStock.Quantity == 0)
            {
                return (false, "There is no stock for the selected product in the source warehouse.");
            }

            if (sourceStock.Quantity < quantity)
            {
                return (false, $"Insufficient stock in the source warehouse. Available quantity is {sourceStock.Quantity}.");
            }

            await _stockMovementDal.BeginTransactionAsync();

            try
            {
                sourceStock.Quantity -= quantity;

                _warehouseStockDal.Update(sourceStock);

                var destinationStock = await _warehouseStockDal
                    .GetByProductAndWarehouseAsync(productId, destinationWarehouseId);

                if (destinationStock == null)
                {
                    destinationStock = new WarehouseStock
                    {
                        ProductId = productId,
                        WarehouseId = destinationWarehouseId,
                        Quantity = quantity
                    };

                    await _warehouseStockDal.AddAsync(destinationStock);
                }
                else
                {
                    destinationStock.Quantity += quantity;

                    _warehouseStockDal.Update(destinationStock);
                }

                var stockMovement = new StockMovement
                {
                    ProductId = productId,
                    SourceWarehouseId = sourceWarehouseId,
                    DestinationWarehouseId = destinationWarehouseId,
                    MovementType = StockMovementType.Transfer,
                    Quantity = quantity,
                    Description = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                    PerformedByUserId = null
                };

                await _stockMovementDal.AddAsync(stockMovement);

                await _warehouseStockDal.SaveChangesAsync();

                await _stockMovementDal.CommitTransactionAsync();

                return (true, null);
            }
            catch
            {
                await _stockMovementDal.RollbackTransactionAsync();

                return (false, "An error occurred during the transfer. The operation was cancelled.");
            }
        }
        public async Task<List<StockMovement>> GetMovementsAsync(
    int? productId,
    int? warehouseId,
    StockMovementType? movementType)
        {
            return await _stockMovementDal.GetMovementsAsync(
                productId,
                warehouseId,
                movementType);
        }
    }
}