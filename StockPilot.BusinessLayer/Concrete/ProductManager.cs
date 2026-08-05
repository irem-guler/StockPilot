using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IProductDal _productDal;

        public ProductManager(IProductDal productDal)
        {
            _productDal = productDal;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _productDal.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _productDal.GetByIdAsync(id);
        }

        public async Task AddAsync(Product product)
        {
            await _productDal.AddAsync(product);
            await _productDal.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _productDal.Update(product);
            await _productDal.SaveChangesAsync();
        }
        public async Task<bool> DeactivateAsync(int id)
        {
            var product = await _productDal.GetByIdAsync(id);

            if (product == null)
            {
                return false;
            }

            product.IsActive = false;

            _productDal.Update(product);
            await _productDal.SaveChangesAsync();

            return true;
        }
    }
}