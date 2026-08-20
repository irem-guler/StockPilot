using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Concrete
{
    public class ReorderManager : IReorderService
    {
        private readonly IProductDal _productDal;
        private readonly IWarehouseStockDal _warehouseStockDal;

        private const int TargetMultiplier = 2;

        public ReorderManager(
            IProductDal productDal,
            IWarehouseStockDal warehouseStockDal)
        {
            _productDal = productDal;
            _warehouseStockDal = warehouseStockDal;
        }

        public async Task<List<ReorderSuggestion>> GetSuggestionsAsync()
        {
            var products = await _productDal.GetAllAsync();

            var inventory = await _warehouseStockDal.GetInventoryAsync(null, null);

            var suggestions = new List<ReorderSuggestion>();

            foreach (var product in products.Where(p => p.IsActive))
            {
                var currentStock = inventory
                    .Where(stock => stock.ProductId == product.ProductId)
                    .Sum(stock => stock.Quantity);

                if (currentStock <= product.ReorderLevel)
                {
                    var targetStock = product.ReorderLevel * TargetMultiplier;

                    var suggestedQuantity = targetStock - currentStock;

                    if (suggestedQuantity < 1)
                    {
                        suggestedQuantity = 1;
                    }

                    suggestions.Add(new ReorderSuggestion
                    {
                        Product = product,
                        CurrentStock = currentStock,
                        SuggestedQuantity = suggestedQuantity
                    });
                }
            }

            return suggestions
                .OrderBy(s => s.CurrentStock)
                .ToList();
        }
    }
}