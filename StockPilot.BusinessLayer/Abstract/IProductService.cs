using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPilot.EntityLayer.Entities;
using StockPilot.BusinessLayer.Models;

namespace StockPilot.BusinessLayer.Abstract
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        Task AddAsync(Product product);

        Task UpdateAsync(Product product);
        Task<bool> DeactivateAsync(int id);
        Task<bool> IsSkuInUseAsync(string sku, int? excludeProductId = null);
        Task<ImportResult> ImportProductsAsync(List<ProductImportRow> rows);
    }
}
