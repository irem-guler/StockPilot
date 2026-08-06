using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Concrete
{
    public class WarehouseManager : IWarehouseService
    {
        private readonly IWarehouseDal _warehouseDal;

        public WarehouseManager(IWarehouseDal warehouseDal)
        {
            _warehouseDal = warehouseDal;
        }

        public async Task<List<Warehouse>> GetAllAsync()
        {
            return await _warehouseDal.GetAllAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int id)
        {
            return await _warehouseDal.GetByIdAsync(id);
        }

        public async Task AddAsync(Warehouse warehouse)
        {
            await _warehouseDal.AddAsync(warehouse);
            await _warehouseDal.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _warehouseDal.Update(warehouse);
            await _warehouseDal.SaveChangesAsync();
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var warehouse = await _warehouseDal.GetByIdAsync(id);

            if (warehouse == null)
            {
                return false;
            }

            warehouse.IsActive = false;

            _warehouseDal.Update(warehouse);
            await _warehouseDal.SaveChangesAsync();

            return true;
        }
    }
}