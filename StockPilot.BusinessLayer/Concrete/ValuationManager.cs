using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Concrete
{
    public class ValuationManager : IValuationService
    {
        private readonly IProductDal _productDal;
        private readonly IWarehouseStockDal _warehouseStockDal;
        private readonly IStockMovementDal _stockMovementDal;

        private const int DeadStockDays = 30;

        public ValuationManager(
            IProductDal productDal,
            IWarehouseStockDal warehouseStockDal,
            IStockMovementDal stockMovementDal)
        {
            _productDal = productDal;
            _warehouseStockDal = warehouseStockDal;
            _stockMovementDal = stockMovementDal;
        }

        public async Task<ValuationResult> GetValuationAsync()
        {
            var products = (await _productDal.GetAllAsync())
                .Where(p => p.IsActive)
                .ToList();

            var inventory = await _warehouseStockDal.GetInventoryAsync(null, null);
            var movements = await _stockMovementDal.GetAllAsync();

            // 1) Her ürünün toplam miktarı ve değeri
            var items = new List<ValuationResultItem>();

            foreach (var product in products)
            {
                var totalQty = inventory
                    .Where(s => s.ProductId == product.ProductId)
                    .Sum(s => s.Quantity);

                var totalValue = totalQty * product.UnitPrice;

                items.Add(new ValuationResultItem
                {
                    Product = product,
                    TotalQuantity = totalQty,
                    TotalValue = totalValue
                });
            }

            // 2) Değere göre azalan sırala
            items = items
                .OrderByDescending(i => i.TotalValue)
                .ToList();

            var grandTotal = items.Sum(i => i.TotalValue);

            // 3) ABC sınıflandırma (kümülatif yüzdeye göre)
            double cumulative = 0;

            foreach (var item in items)
            {
                var pct = grandTotal > 0
                    ? (double)(item.TotalValue / grandTotal) * 100
                    : 0;

                cumulative += pct;

                item.ValuePercentage = Math.Round(pct, 2);
                item.CumulativePercentage = Math.Round(cumulative, 2);

                if (cumulative <= 80)
                {
                    item.AbcClass = "A";
                }
                else if (cumulative <= 95)
                {
                    item.AbcClass = "B";
                }
                else
                {
                    item.AbcClass = "C";
                }
            }

            // 4) Ölü stok: stoğu olan ama son 30 gündür hareketi olmayan ürünler
            var now = DateTime.UtcNow;
            var deadStock = new List<DeadStockResultItem>();

            foreach (var item in items.Where(i => i.TotalQuantity > 0))
            {
                var lastMovement = movements
                    .Where(m => m.ProductId == item.Product.ProductId)
                    .OrderByDescending(m => m.MovementDateUtc)
                    .FirstOrDefault();

                var lastDate = lastMovement?.MovementDateUtc;
                var days = lastDate.HasValue
                    ? (int)(now - lastDate.Value).TotalDays
                    : int.MaxValue;

                if (days >= DeadStockDays)
                {
                    deadStock.Add(new DeadStockResultItem
                    {
                        Product = item.Product,
                        TotalQuantity = item.TotalQuantity,
                        TotalValue = item.TotalValue,
                        LastMovementDate = lastDate,
                        DaysSinceLastMovement = days == int.MaxValue ? -1 : days
                    });
                }
            }

            deadStock = deadStock
                .OrderByDescending(d => d.TotalValue)
                .ToList();

            return new ValuationResult
            {
                Items = items,
                DeadStock = deadStock
            };
        }
    }
}