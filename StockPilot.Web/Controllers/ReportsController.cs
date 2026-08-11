using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.BusinessLayer.Abstract;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public ReportsController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
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
    }
}