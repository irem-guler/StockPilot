using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.Web.Services
{
    public class OrderPdfService
    {
        public byte[] GenerateSalesOrderPdf(SalesOrder order)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    ComposeHeader(page, "DELIVERY NOTE / INVOICE", $"Sales Order #{order.SalesOrderId}");

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Customer").Bold();
                                c.Item().Text(order.Customer?.Name ?? "-");
                                c.Item().Text(order.Customer?.Address ?? "");
                                c.Item().Text(order.Customer?.Phone ?? "");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text($"Date: {order.OrderDateUtc:yyyy-MM-dd}");
                                c.Item().AlignRight().Text($"Warehouse: {order.Warehouse?.Name ?? "-"}");
                                c.Item().AlignRight().Text($"Status: {order.Status}");
                            });
                        });

                        ComposeItemsTable(col,
                            order.Items.Select(i => (
                                i.Product?.Name ?? "-",
                                i.Product?.SKU ?? "-",
                                i.Quantity,
                                i.UnitPrice)),
                            order.TotalAmount);

                        if (!string.IsNullOrWhiteSpace(order.Note))
                        {
                            col.Item().PaddingTop(10).Text($"Note: {order.Note}").Italic();
                        }
                    });

                    ComposeFooter(page);
                });
            }).GeneratePdf();
        }

        public byte[] GeneratePurchaseOrderPdf(PurchaseOrder order)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    ComposeHeader(page, "PURCHASE ORDER", $"Purchase Order #{order.PurchaseOrderId}");

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Supplier").Bold();
                                c.Item().Text(order.Supplier?.Name ?? "-");
                                c.Item().Text(order.Supplier?.Address ?? "");
                                c.Item().Text(order.Supplier?.Phone ?? "");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().AlignRight().Text($"Date: {order.OrderDateUtc:yyyy-MM-dd}");
                                c.Item().AlignRight().Text($"Warehouse: {order.Warehouse?.Name ?? "-"}");
                                c.Item().AlignRight().Text($"Status: {order.Status}");
                            });
                        });

                        ComposeItemsTable(col,
                            order.Items.Select(i => (
                                i.Product?.Name ?? "-",
                                i.Product?.SKU ?? "-",
                                i.Quantity,
                                i.UnitPrice)),
                            order.TotalAmount);

                        if (!string.IsNullOrWhiteSpace(order.Note))
                        {
                            col.Item().PaddingTop(10).Text($"Note: {order.Note}").Italic();
                        }
                    });

                    ComposeFooter(page);
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(PageDescriptor page, string documentTitle, string orderRef)
        {
            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("StockPilot").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        c.Item().Text("Inventory Management System").FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(documentTitle).FontSize(14).Bold();
                        c.Item().AlignRight().Text(orderRef).FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });

                col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
        }

        private void ComposeItemsTable(
            ColumnDescriptor col,
            IEnumerable<(string Name, string SKU, int Quantity, decimal UnitPrice)> items,
            decimal total)
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Product").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("SKU").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Qty").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Unit Price").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                });

                foreach (var item in items)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Name);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.SKU);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.UnitPrice.ToString("N2"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text((item.Quantity * item.UnitPrice).ToString("N2"));
                }
            });

            col.Item().AlignRight().PaddingTop(10).Text($"Grand Total: {total:N2}").FontSize(13).Bold();
        }

        private void ComposeFooter(PageDescriptor page)
        {
            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Generated by StockPilot on ").FontSize(8).FontColor(Colors.Grey.Medium);
                x.Span($"{DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }
    }
}