using StockPilot.BusinessLayer.Abstract;

namespace StockPilot.Web.Services
{
    public class AssistantDataService
    {
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IInventoryService _inventoryService;
        private readonly ISalesOrderService _salesOrderService;
        private readonly IValuationService _valuationService;

        public AssistantDataService(
            IProductService productService,
            IWarehouseService warehouseService,
            IInventoryService inventoryService,
            ISalesOrderService salesOrderService,
            IValuationService valuationService)
        {
            _productService = productService;
            _warehouseService = warehouseService;
            _inventoryService = inventoryService;
            _salesOrderService = salesOrderService;
            _valuationService = valuationService;
        }

        public async Task<string> GetInventorySummaryAsync()
        {
            var products = await _productService.GetAllAsync();
            var warehouses = await _warehouseService.GetAllAsync();
            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            var activeProducts = products.Count(p => p.IsActive);
            var activeWarehouses = warehouses.Count(w => w.IsActive);
            var totalStock = inventory.Sum(s => s.Quantity);
            var criticalCount = inventory.Count(s => s.Quantity <= s.Product.ReorderLevel);

            return $"Total active products: {activeProducts}. " +
                   $"Total active warehouses: {activeWarehouses}. " +
                   $"Total units in stock: {totalStock}. " +
                   $"Product-warehouse records at or below reorder level: {criticalCount}.";
        }

        public async Task<string> GetCriticalStockAsync()
        {
            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            var critical = inventory
                .Where(s => s.Quantity <= s.Product.ReorderLevel)
                .OrderBy(s => s.Quantity)
                .Take(20)
                .Select(s => $"{s.Product.Name} ({s.Product.SKU}) in {s.Warehouse.Name}: " +
                             $"{s.Quantity} units (reorder level {s.Product.ReorderLevel})")
                .ToList();

            if (critical.Count == 0)
            {
                return "No products are currently at or below their reorder level.";
            }

            return "Critical stock items:\n" + string.Join("\n", critical);
        }

        public async Task<string> SearchProductAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return "No search term provided.";
            }

            var inventory = await _inventoryService.GetInventoryAsync(null, null);
            var q = query.ToLower();

            var matches = inventory
                .Where(s => (s.Product.Name != null && s.Product.Name.ToLower().Contains(q))
                    || (s.Product.SKU != null && s.Product.SKU.ToLower().Contains(q)))
                .GroupBy(s => new { s.Product.Name, s.Product.SKU, s.Product.UnitPrice })
                .Select(g => $"{g.Key.Name} ({g.Key.SKU}), unit price {g.Key.UnitPrice:N2}: " +
                             $"total {g.Sum(x => x.Quantity)} units across " +
                             string.Join(", ", g.Select(x => $"{x.Warehouse.Name} ({x.Quantity})")))
                .Take(10)
                .ToList();

            if (matches.Count == 0)
            {
                return $"No products found matching '{query}'.";
            }

            return string.Join("\n", matches);
        }

        public async Task<string> GetWarehouseStockAsync(string warehouseName)
        {
            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            if (string.IsNullOrWhiteSpace(warehouseName))
            {
                return "No warehouse name provided.";
            }

            var q = warehouseName.ToLower();
            var stocks = inventory
                .Where(s => s.Warehouse.Name != null && s.Warehouse.Name.ToLower().Contains(q))
                .ToList();

            if (stocks.Count == 0)
            {
                return $"No warehouse found matching '{warehouseName}'.";
            }

            var warehouseGroup = stocks.GroupBy(s => s.Warehouse.Name).First();
            var productCount = warehouseGroup.Count(s => s.Quantity > 0);
            var totalStock = warehouseGroup.Sum(s => s.Quantity);
            var critical = warehouseGroup.Count(s => s.Quantity <= s.Product.ReorderLevel);

            return $"{warehouseGroup.Key}: {productCount} products with stock, " +
                   $"{totalStock} total units, {critical} items at or below reorder level.";
        }

        public async Task<string> GetTopSellingAsync()
        {
            var orders = await _salesOrderService.GetAllAsync();

            var top = orders
                .Where(o => o.Status != EntityLayer.Enums.SalesOrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Product != null ? i.Product.Name : "Unknown")
                .Select(g => new { Name = g.Key, Total = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            if (top.Count == 0)
            {
                return "No sales data available yet.";
            }

            return "Top selling products:\n" +
                   string.Join("\n", top.Select(t => $"{t.Name}: {t.Total} units sold"));
        }

        public async Task<string> GetSalesSummaryAsync()
        {
            var orders = await _salesOrderService.GetAllAsync();

            var shipped = orders.Where(o => o.Status == EntityLayer.Enums.SalesOrderStatus.Shipped).ToList();
            var pending = orders.Count(o => o.Status == EntityLayer.Enums.SalesOrderStatus.Pending);

            var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthlyTotal = orders
                .Where(o => o.OrderDateUtc >= firstOfMonth
                    && o.Status != EntityLayer.Enums.SalesOrderStatus.Cancelled)
                .Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));

            var totalShippedValue = shipped.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));

            return $"Total sales orders: {orders.Count}. Shipped: {shipped.Count}, Pending: {pending}. " +
                   $"This month's order value (excluding cancelled): {monthlyTotal:N2}. " +
                   $"Total shipped value: {totalShippedValue:N2}.";
        }

        public async Task<string> GetInventoryValueAsync()
        {
            var result = await _valuationService.GetValuationAsync();

            var totalValue = result.Items.Sum(i => i.TotalValue);
            var classA = result.Items.Count(i => i.AbcClass == "A");
            var classB = result.Items.Count(i => i.AbcClass == "B");
            var classC = result.Items.Count(i => i.AbcClass == "C");

            var topValue = result.Items
                .OrderByDescending(i => i.TotalValue)
                .Take(3)
                .Select(i => $"{i.Product.Name} ({i.TotalValue:N2})")
                .ToList();

            return $"Total inventory value: {totalValue:N2}. " +
                   $"ABC classes — A: {classA} products, B: {classB}, C: {classC}. " +
                   $"Most valuable products: {string.Join(", ", topValue)}.";
        }
        public async Task<string> GetTransferHistoryAsync()
        {
            var movements = await _inventoryService.GetMovementsAsync(
                null, null, StockPilot.EntityLayer.Enums.StockMovementType.Transfer);

            var flows = movements
                .Where(m => m.SourceWarehouse != null && m.DestinationWarehouse != null)
                .GroupBy(m => new
                {
                    From = m.SourceWarehouse!.Name,
                    To = m.DestinationWarehouse!.Name
                })
                .Select(g => new
                {
                    g.Key.From,
                    g.Key.To,
                    Total = g.Sum(m => m.Quantity),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(15)
                .ToList();

            if (flows.Count == 0)
            {
                return "No transfers between warehouses have been recorded yet.";
            }

            return "Transfer history between warehouses:\n" +
                   string.Join("\n", flows.Select(f =>
                       $"{f.From} -> {f.To}: {f.Total} units in {f.Count} transfer(s)"));
        }

        public async Task<string> GetTransferSuggestionsAsync()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            var coordWarehouses = warehouses
                .Where(w => w.IsActive && w.Latitude.HasValue && w.Longitude.HasValue)
                .ToList();

            var criticalStocks = inventory
                .Where(s => coordWarehouses.Any(w => w.WarehouseId == s.WarehouseId)
                    && s.Quantity <= s.Product.ReorderLevel)
                .ToList();

            var suggestions = new List<string>();

            foreach (var critical in criticalStocks)
            {
                var target = coordWarehouses.First(w => w.WarehouseId == critical.WarehouseId);

                var candidates = inventory
                    .Where(s => s.ProductId == critical.ProductId
                        && s.WarehouseId != critical.WarehouseId
                        && coordWarehouses.Any(w => w.WarehouseId == s.WarehouseId)
                        && (s.Quantity - s.ReservedQuantity) > s.Product.ReorderLevel)
                    .ToList();

                if (candidates.Count == 0)
                {
                    continue;
                }

                string? nearestName = null;
                double nearestKm = double.MaxValue;
                int sourceAvailable = 0;

                foreach (var candidate in candidates)
                {
                    var cw = coordWarehouses.First(w => w.WarehouseId == candidate.WarehouseId);
                    var km = Haversine(
                        target.Latitude!.Value, target.Longitude!.Value,
                        cw.Latitude!.Value, cw.Longitude!.Value);

                    if (km < nearestKm)
                    {
                        nearestKm = km;
                        nearestName = cw.Name;
                        sourceAvailable = candidate.Quantity - candidate.ReservedQuantity;
                    }
                }

                if (nearestName == null)
                {
                    continue;
                }

                var targetGoal = critical.Product.ReorderLevel * 2 - critical.Quantity;
                var canGive = sourceAvailable - critical.Product.ReorderLevel;
                var qty = Math.Min(targetGoal, canGive);

                if (qty < 1)
                {
                    continue;
                }

                suggestions.Add(
                    $"{critical.Product.Name} ({critical.Product.SKU}) is critical in {target.Name} " +
                    $"({critical.Quantity} units). Suggested: transfer {qty} units from {nearestName} " +
                    $"(nearest source, about {Math.Round(nearestKm)} km away).");
            }

            if (suggestions.Count == 0)
            {
                return "No transfer suggestions at the moment. No critical stock has a suitable nearby source.";
            }

            return "Transfer suggestions:\n" + string.Join("\n", suggestions.Take(15));
        }

        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double r = 6371.0;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}