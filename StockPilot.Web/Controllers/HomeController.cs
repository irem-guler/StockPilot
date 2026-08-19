using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.Web.Models;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IInventoryService _inventoryService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesOrderService _salesOrderService;

        public HomeController(
            IProductService productService,
            IWarehouseService warehouseService,
            IInventoryService inventoryService,
            IPurchaseOrderService purchaseOrderService,
            ISalesOrderService salesOrderService)
        {
            _productService = productService;
            _warehouseService = warehouseService;
            _inventoryService = inventoryService;
            _purchaseOrderService = purchaseOrderService;
            _salesOrderService = salesOrderService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            var warehouses = await _warehouseService.GetAllAsync();

            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            var movements = await _inventoryService.GetMovementsAsync(
                null, null, null);

            var criticalStocks = inventory
                .Where(stock => stock.Quantity <= stock.Product.ReorderLevel)
                .OrderBy(stock => stock.Quantity)
                .ToList();


            var stockInCount = movements.Count(m => m.MovementType == StockMovementType.StockIn);
            var stockOutCount = movements.Count(m => m.MovementType == StockMovementType.StockOut);
            var transferCount = movements.Count(m => m.MovementType == StockMovementType.Transfer);


            var warehouseGroups = inventory
                .GroupBy(stock => stock.Warehouse.Name)
                .Select(group => new
                {
                    Name = group.Key,
                    TotalQuantity = group.Sum(stock => stock.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(8)
                .ToList();


            var today = DateTime.UtcNow.Date;

            var last7Days = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(-6 + offset))
                .ToList();

            var movementDays = last7Days
                .Select(day => day.ToString("MM-dd"))
                .ToList();

            var movementDayCounts = last7Days
                .Select(day => movements.Count(m => m.MovementDateUtc.Date == day))
                .ToList();

            var purchaseOrders = await _purchaseOrderService.GetAllAsync();
            var salesOrders = await _salesOrderService.GetAllAsync();

            var firstDayOfMonth = new DateTime(
                DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var pendingPurchaseCount = purchaseOrders
                .Count(o => o.Status == PurchaseOrderStatus.Pending);

            var pendingSalesCount = salesOrders
                .Count(o => o.Status == SalesOrderStatus.Pending);

            var monthlyPurchaseTotal = purchaseOrders
                .Where(o => o.OrderDateUtc >= firstDayOfMonth
                    && o.Status != PurchaseOrderStatus.Cancelled)
                .Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));

            var monthlySalesTotal = salesOrders
                .Where(o => o.OrderDateUtc >= firstDayOfMonth
                    && o.Status != SalesOrderStatus.Cancelled)
                .Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));
            var topProducts = salesOrders
                .Where(o => o.Status != SalesOrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .GroupBy(i => i.Product != null ? i.Product.Name : "Unknown")
                .Select(g => new { Name = g.Key, Total = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalProductCount = products.Count(product => product.IsActive),
                TotalWarehouseCount = warehouses.Count(warehouse => warehouse.IsActive),
                CriticalStockCount = criticalStocks.Count,
                TotalMovementCount = movements.Count,
                CriticalStocks = criticalStocks.Take(10).ToList(),
                RecentMovements = movements.Take(10).ToList(),

                StockInCount = stockInCount,
                StockOutCount = stockOutCount,
                TransferCount = transferCount,

                WarehouseNames = warehouseGroups.Select(x => x.Name).ToList(),
                WarehouseQuantities = warehouseGroups.Select(x => x.TotalQuantity).ToList(),

                MovementDays = movementDays,
                MovementDayCounts = movementDayCounts,

                PendingPurchaseOrderCount = pendingPurchaseCount,
                PendingSalesOrderCount = pendingSalesCount,
                MonthlyPurchaseTotal = monthlyPurchaseTotal,
                MonthlySalesTotal = monthlySalesTotal,
                PurchaseOrderCount = purchaseOrders.Count,
                SalesOrderCount = salesOrders.Count,

                TopProductNames = topProducts.Select(x => x.Name).ToList(),
                TopProductQuantities = topProducts.Select(x => x.Total).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}