using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Entities;
using StockPilot.EntityLayer.Enums;
using StockPilot.Web.Models;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class MapController : Controller
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IInventoryService _inventoryService;
        private readonly StockPilot.Web.Services.DistanceService _distanceService;

        public MapController(
            IWarehouseService warehouseService,
            IInventoryService inventoryService,
            StockPilot.Web.Services.DistanceService distanceService)
        {
            _warehouseService = warehouseService;
            _inventoryService = inventoryService;
            _distanceService = distanceService;
        }

        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            var points = new List<WarehouseMapPoint>();

            foreach (var warehouse in warehouses.Where(w =>
                w.IsActive && w.Latitude.HasValue && w.Longitude.HasValue))
            {
                var warehouseStocks = inventory
                    .Where(stock => stock.WarehouseId == warehouse.WarehouseId)
                    .ToList();

                var totalStock = warehouseStocks.Sum(s => s.Quantity);
                var productCount = warehouseStocks.Count(s => s.Quantity > 0);
                var criticalCount = warehouseStocks
                    .Count(s => s.Quantity <= s.Product.ReorderLevel);

                points.Add(new WarehouseMapPoint
                {
                    WarehouseId = warehouse.WarehouseId,
                    Name = warehouse.Name,
                    Location = warehouse.Location,
                    Latitude = warehouse.Latitude!.Value,
                    Longitude = warehouse.Longitude!.Value,
                    TotalStock = totalStock,
                    ProductCount = productCount,
                    CriticalCount = criticalCount
                });
            }

            
            var movements = await _inventoryService.GetMovementsAsync(
                null, null, StockMovementType.Transfer);

            var warehouseLookup = warehouses
                .Where(w => w.Latitude.HasValue && w.Longitude.HasValue)
                .ToDictionary(w => w.WarehouseId);

            var flows = movements
                .Where(m => m.SourceWarehouseId.HasValue
                    && m.DestinationWarehouseId.HasValue
                    && warehouseLookup.ContainsKey(m.SourceWarehouseId.Value)
                    && warehouseLookup.ContainsKey(m.DestinationWarehouseId.Value))
                .GroupBy(m => new { m.SourceWarehouseId, m.DestinationWarehouseId })
                .Select(g =>
                {
                    var source = warehouseLookup[g.Key.SourceWarehouseId!.Value];
                    var dest = warehouseLookup[g.Key.DestinationWarehouseId!.Value];

                    return new TransferFlow
                    {
                        FromLatitude = source.Latitude!.Value,
                        FromLongitude = source.Longitude!.Value,
                        ToLatitude = dest.Latitude!.Value,
                        ToLongitude = dest.Longitude!.Value,
                        FromName = source.Name,
                        ToName = dest.Name,
                        TotalQuantity = g.Sum(m => m.Quantity),
                        TransferCount = g.Count()
                    };
                })
                .ToList();

            // 3) Mesafe bazlı transfer önerileri
            var suggestions = new List<Models.TransferSuggestion>();

            var coordWarehouses = warehouses
                .Where(w => w.IsActive && w.Latitude.HasValue && w.Longitude.HasValue)
                .ToList();

            // Kritik olan (stok <= reorder) depo-ürün kayıtları
            var criticalStocks = inventory
                .Where(s => coordWarehouses.Any(w => w.WarehouseId == s.WarehouseId)
                    && s.Quantity <= s.Product.ReorderLevel)
                .ToList();

            foreach (var critical in criticalStocks)
            {
                var targetWarehouse = coordWarehouses
                    .First(w => w.WarehouseId == critical.WarehouseId);

                // Aynı üründen fazla stoğu olan aday kaynak depolar
                // (kaynakta available > reorder, yani verince kendisi kritik olmayacak)
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

                // Kuş uçuşuyla en yakın adayı seç (OSRM'i sadece ona soracağız)
                Warehouse? nearest = null;
                double nearestRoughKm = double.MaxValue;
                WarehouseStock? nearestStock = null;

                foreach (var candidate in candidates)
                {
                    var candidateWarehouse = coordWarehouses
                        .First(w => w.WarehouseId == candidate.WarehouseId);

                    var roughKm = Haversine(
                        targetWarehouse.Latitude!.Value, targetWarehouse.Longitude!.Value,
                        candidateWarehouse.Latitude!.Value, candidateWarehouse.Longitude!.Value);

                    if (roughKm < nearestRoughKm)
                    {
                        nearestRoughKm = roughKm;
                        nearest = candidateWarehouse;
                        nearestStock = candidate;
                    }
                }

                if (nearest == null || nearestStock == null)
                {
                    continue;
                }

                // Sadece en yakın aday için gerçek yol mesafesi (OSRM)
                var distance = await _distanceService.GetDistanceAsync(
                    nearest.Latitude!.Value, nearest.Longitude!.Value,
                    targetWarehouse.Latitude!.Value, targetWarehouse.Longitude!.Value);

                // Önerilen miktar: hedefi reorder×2'ye tamamla, kaynağın verebileceğiyle sınırla
                var targetGoal = critical.Product.ReorderLevel * 2 - critical.Quantity;
                var sourceCanGive = (nearestStock.Quantity - nearestStock.ReservedQuantity)
                    - nearestStock.Product.ReorderLevel;
                var suggestedQty = Math.Min(targetGoal, sourceCanGive);

                if (suggestedQty < 1)
                {
                    continue;
                }

                suggestions.Add(new Models.TransferSuggestion
                {
                    ProductName = critical.Product.Name,
                    SKU = critical.Product.SKU,
                    FromWarehouseName = nearest.Name,
                    ToWarehouseName = targetWarehouse.Name,
                    SuggestedQuantity = suggestedQty,
                    SourceAvailable = nearestStock.Quantity - nearestStock.ReservedQuantity,
                    TargetCurrent = critical.Quantity,
                    DistanceKm = distance.DistanceKm,
                    DurationMinutes = distance.DurationMinutes,
                    IsApproximate = distance.IsApproximate,
                    FromLat = nearest.Latitude!.Value,
                    FromLng = nearest.Longitude!.Value,
                    ToLat = targetWarehouse.Latitude!.Value,
                    ToLng = targetWarehouse.Longitude!.Value
                });
            }

            suggestions = suggestions
                .OrderBy(s => s.DistanceKm)
                .ToList();

            var viewModel = new WarehouseMapViewModel
            {
                Points = points,
                Flows = flows,
                Suggestions = suggestions
            };

            return View(viewModel);
        }
        private static double Haversine(
    double lat1, double lon1, double lat2, double lon2)
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