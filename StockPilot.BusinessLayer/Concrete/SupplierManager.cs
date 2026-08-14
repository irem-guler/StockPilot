using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Concrete
{
    public class SupplierManager : ISupplierService
    {
        private readonly ISupplierDal _supplierDal;

        public SupplierManager(ISupplierDal supplierDal)
        {
            _supplierDal = supplierDal;
        }

        public async Task<List<Supplier>> GetAllAsync()
        {
            return await _supplierDal.GetAllAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _supplierDal.GetByIdAsync(id);
        }

        public async Task AddAsync(Supplier supplier)
        {
            await _supplierDal.AddAsync(supplier);
            await _supplierDal.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _supplierDal.Update(supplier);
            await _supplierDal.SaveChangesAsync();
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var supplier = await _supplierDal.GetByIdAsync(id);

            if (supplier == null)
            {
                return false;
            }

            supplier.IsActive = false;

            _supplierDal.Update(supplier);
            await _supplierDal.SaveChangesAsync();

            return true;
        }
    }
}