using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.BusinessLayer.Models;

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
        public async Task<bool> IsSkuInUseAsync(
    string sku,
    int? excludeProductId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return false;
            }

            var normalizedSku = sku.Trim();

            var allProducts = await _productDal.GetAllAsync();

            return allProducts.Any(product =>
                product.SKU != null &&
                product.SKU.Trim().ToUpper() == normalizedSku.ToUpper() &&
                product.ProductId != excludeProductId);
        }
        public async Task<ImportResult> ImportProductsAsync(
    List<ProductImportRow> rows)
        {
            var result = new ImportResult();

            var existingProducts = await _productDal.GetAllAsync();

            var existingSkus = existingProducts
                .Where(product => product.SKU != null)
                .Select(product => product.SKU.Trim().ToUpper())
                .ToHashSet();

            var seenSkusInFile = new HashSet<string>();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    result.FailureCount++;
                    result.Errors.Add($"Row {row.RowNumber}: Product name is required.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.SKU))
                {
                    result.FailureCount++;
                    result.Errors.Add($"Row {row.RowNumber}: SKU is required.");
                    continue;
                }

                var normalizedSku = row.SKU.Trim().ToUpper();

                if (existingSkus.Contains(normalizedSku))
                {
                    result.FailureCount++;
                    result.Errors.Add($"Row {row.RowNumber}: SKU '{row.SKU}' already exists in the system.");
                    continue;
                }

                if (seenSkusInFile.Contains(normalizedSku))
                {
                    result.FailureCount++;
                    result.Errors.Add($"Row {row.RowNumber}: SKU '{row.SKU}' is duplicated within the file.");
                    continue;
                }

                if (row.UnitPrice < 0)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Row {row.RowNumber}: Unit price cannot be negative.");
                    continue;
                }

                if (row.ReorderLevel < 0)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Row {row.RowNumber}: Reorder level cannot be negative.");
                    continue;
                }

                var product = new Product
                {
                    Name = row.Name.Trim(),
                    SKU = normalizedSku,
                    Description = string.IsNullOrWhiteSpace(row.Description)
                        ? null
                        : row.Description.Trim(),
                    UnitPrice = row.UnitPrice,
                    ReorderLevel = row.ReorderLevel,
                    IsActive = true
                };

                await _productDal.AddAsync(product);

                seenSkusInFile.Add(normalizedSku);
                result.SuccessCount++;
            }

            if (result.SuccessCount > 0)
            {
                await _productDal.SaveChangesAsync();
            }

            return result;
        }
    }
}