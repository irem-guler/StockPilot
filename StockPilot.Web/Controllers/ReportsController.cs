using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;
using StockPilot.EntityLayer.Enums;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISalesOrderService _salesOrderService;

        public ReportsController(
            IInventoryService inventoryService,
            IPurchaseOrderService purchaseOrderService,
            ISalesOrderService salesOrderService)
        {
            _inventoryService = inventoryService;
            _purchaseOrderService = purchaseOrderService;
            _salesOrderService = salesOrderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> StockStatusExcel()
        {
            var inventory = await _inventoryService.GetInventoryAsync(null, null);

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Stock Status");

            worksheet.Cell(1, 1).Value = "Product";
            worksheet.Cell(1, 2).Value = "SKU";
            worksheet.Cell(1, 3).Value = "Warehouse";
            worksheet.Cell(1, 4).Value = "Quantity";
            worksheet.Cell(1, 5).Value = "Reorder Level";
            worksheet.Cell(1, 6).Value = "Status";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            var currentRow = 2;

            foreach (var stock in inventory
                .OrderBy(s => s.Product.Name)
                .ThenBy(s => s.Warehouse.Name))
            {
                worksheet.Cell(currentRow, 1).Value = stock.Product.Name;
                worksheet.Cell(currentRow, 2).Value = stock.Product.SKU;
                worksheet.Cell(currentRow, 3).Value = stock.Warehouse.Name;
                worksheet.Cell(currentRow, 4).Value = stock.Quantity;
                worksheet.Cell(currentRow, 5).Value = stock.Product.ReorderLevel;

                var status = stock.Quantity == 0
                    ? "Out of Stock"
                    : stock.Quantity <= stock.Product.ReorderLevel
                        ? "Critical"
                        : "In Stock";

                worksheet.Cell(currentRow, 6).Value = status;

                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var fileName =
                $"StockStatus_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        public async Task<IActionResult> MovementsExcel(
            DateTime? startDate,
            DateTime? endDate)
        {
            var movements = await _inventoryService.GetMovementsAsync(
                null, null, null);

            if (startDate.HasValue)
            {
                movements = movements
                    .Where(m => m.MovementDateUtc >= startDate.Value.Date)
                    .ToList();
            }

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.Date.AddDays(1);

                movements = movements
                    .Where(m => m.MovementDateUtc < inclusiveEnd)
                    .ToList();
            }

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Movements");

            worksheet.Cell(1, 1).Value = "Date (UTC)";
            worksheet.Cell(1, 2).Value = "Product";
            worksheet.Cell(1, 3).Value = "SKU";
            worksheet.Cell(1, 4).Value = "Type";
            worksheet.Cell(1, 5).Value = "Source Warehouse";
            worksheet.Cell(1, 6).Value = "Destination Warehouse";
            worksheet.Cell(1, 7).Value = "Quantity";
            worksheet.Cell(1, 8).Value = "User";
            worksheet.Cell(1, 9).Value = "Note";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            var currentRow = 2;

            foreach (var movement in movements)
            {
                worksheet.Cell(currentRow, 1).Value =
                    movement.MovementDateUtc.ToString(
                        "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                worksheet.Cell(currentRow, 2).Value =
                    movement.Product?.Name ?? "-";

                worksheet.Cell(currentRow, 3).Value =
                    movement.Product?.SKU ?? "-";

                worksheet.Cell(currentRow, 4).Value =
                    movement.MovementType.ToString();

                worksheet.Cell(currentRow, 5).Value =
                    movement.SourceWarehouse?.Name ?? "-";

                worksheet.Cell(currentRow, 6).Value =
                    movement.DestinationWarehouse?.Name ?? "-";

                worksheet.Cell(currentRow, 7).Value = movement.Quantity;

                worksheet.Cell(currentRow, 8).Value =
                    movement.PerformedByUser != null
                        ? $"{movement.PerformedByUser.FirstName} {movement.PerformedByUser.LastName}".Trim()
                        : "-";

                worksheet.Cell(currentRow, 9).Value =
                    string.IsNullOrWhiteSpace(movement.Description)
                        ? "-"
                        : movement.Description;

                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var fileName =
                $"Movements_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        public async Task<IActionResult> PurchaseOrdersExcel(
    DateTime? startDate,
    DateTime? endDate)
        {
            var orders = await _purchaseOrderService.GetAllAsync();

            if (startDate.HasValue)
            {
                orders = orders
                    .Where(o => o.OrderDateUtc >= startDate.Value.Date)
                    .ToList();
            }

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.Date.AddDays(1);
                orders = orders
                    .Where(o => o.OrderDateUtc < inclusiveEnd)
                    .ToList();
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Purchase Orders");

            worksheet.Cell(1, 1).Value = "Order #";
            worksheet.Cell(1, 2).Value = "Order Date";
            worksheet.Cell(1, 3).Value = "Supplier";
            worksheet.Cell(1, 4).Value = "Warehouse";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Product";
            worksheet.Cell(1, 7).Value = "SKU";
            worksheet.Cell(1, 8).Value = "Quantity";
            worksheet.Cell(1, 9).Value = "Unit Price";
            worksheet.Cell(1, 10).Value = "Line Total";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            var currentRow = 2;

            foreach (var order in orders.OrderByDescending(o => o.OrderDateUtc))
            {
                foreach (var item in order.Items)
                {
                    worksheet.Cell(currentRow, 1).Value = order.PurchaseOrderId;
                    worksheet.Cell(currentRow, 2).Value =
                        order.OrderDateUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    worksheet.Cell(currentRow, 3).Value = order.Supplier?.Name ?? "-";
                    worksheet.Cell(currentRow, 4).Value = order.Warehouse?.Name ?? "-";
                    worksheet.Cell(currentRow, 5).Value = order.Status.ToString();
                    worksheet.Cell(currentRow, 6).Value = item.Product?.Name ?? "-";
                    worksheet.Cell(currentRow, 7).Value = item.Product?.SKU ?? "-";
                    worksheet.Cell(currentRow, 8).Value = item.Quantity;
                    worksheet.Cell(currentRow, 9).Value = item.UnitPrice;
                    worksheet.Cell(currentRow, 10).Value = item.Quantity * item.UnitPrice;

                    currentRow++;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var fileName = $"PurchaseOrders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        public async Task<IActionResult> SalesOrdersExcel(
    DateTime? startDate,
    DateTime? endDate)
        {
            var orders = await _salesOrderService.GetAllAsync();

            if (startDate.HasValue)
            {
                orders = orders
                    .Where(o => o.OrderDateUtc >= startDate.Value.Date)
                    .ToList();
            }

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.Date.AddDays(1);
                orders = orders
                    .Where(o => o.OrderDateUtc < inclusiveEnd)
                    .ToList();
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sales Orders");

            worksheet.Cell(1, 1).Value = "Order #";
            worksheet.Cell(1, 2).Value = "Order Date";
            worksheet.Cell(1, 3).Value = "Customer";
            worksheet.Cell(1, 4).Value = "Warehouse";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Product";
            worksheet.Cell(1, 7).Value = "SKU";
            worksheet.Cell(1, 8).Value = "Quantity";
            worksheet.Cell(1, 9).Value = "Unit Price";
            worksheet.Cell(1, 10).Value = "Line Total";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            var currentRow = 2;

            foreach (var order in orders.OrderByDescending(o => o.OrderDateUtc))
            {
                foreach (var item in order.Items)
                {
                    worksheet.Cell(currentRow, 1).Value = order.SalesOrderId;
                    worksheet.Cell(currentRow, 2).Value =
                        order.OrderDateUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    worksheet.Cell(currentRow, 3).Value = order.Customer?.Name ?? "-";
                    worksheet.Cell(currentRow, 4).Value = order.Warehouse?.Name ?? "-";
                    worksheet.Cell(currentRow, 5).Value = order.Status.ToString();
                    worksheet.Cell(currentRow, 6).Value = item.Product?.Name ?? "-";
                    worksheet.Cell(currentRow, 7).Value = item.Product?.SKU ?? "-";
                    worksheet.Cell(currentRow, 8).Value = item.Quantity;
                    worksheet.Cell(currentRow, 9).Value = item.UnitPrice;
                    worksheet.Cell(currentRow, 10).Value = item.Quantity * item.UnitPrice;

                    currentRow++;
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            var fileName = $"SalesOrders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}