using Application.DTOs;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Infrastructure.Services
{
    /// <summary>خدمة تصدير بيانات العملاء إلى Excel و PDF</summary>
    public class CustomerExportService
    {
        /// <summary>تصدير قائمة العملاء إلى Excel</summary>
        public byte[] ExportToExcel(List<CustomerDto> customers)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("العملاء");

            ws.RightToLeft = true;

            var headers = new[]
            {
                "م", "رقم العميل", "الاسم", "النوع",
                "البريد الإلكتروني", "الهاتف", "العنوان",
                "عدد المجموعات", "تاريخ الإضافة"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 12;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Row(1).Height = 30;

            for (int r = 0; r < customers.Count; r++)
            {
                var c = customers[r];
                var rowNum = r + 2;
                var row = ws.Row(rowNum);

                if (r % 2 == 1)
                    row.Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f4f8");

                ws.Cell(rowNum, 1).Value = r + 1;
                ws.Cell(rowNum, 2).Value = c.CustomerNumber;
                ws.Cell(rowNum, 3).Value = c.Name;
                ws.Cell(rowNum, 4).Value = c.CustomerTypeDisplay;
                ws.Cell(rowNum, 5).Value = c.Email ?? "-";
                ws.Cell(rowNum, 6).Value = c.Phone ?? "-";
                ws.Cell(rowNum, 7).Value = c.Address ?? "-";
                ws.Cell(rowNum, 8).Value = c.TrapGroupCount;
                ws.Cell(rowNum, 9).Value = c.CreatedAt.ToString("yyyy-MM-dd");

                row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();
            ws.Column(7).Width = Math.Max(ws.Column(7).Width, 30);

            var summaryRow = customers.Count + 2;
            ws.Cell(summaryRow, 1).Value = "الإجمالي:";
            ws.Cell(summaryRow, 8).Value = customers.Sum(c => c.TrapGroupCount);
            ws.Row(summaryRow).Style.Font.Bold = true;
            ws.Row(summaryRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f4fd");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>تصدير قائمة العملاء إلى PDF</summary>
        public byte[] ExportToPdf(List<CustomerDto> customers)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, customers));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("صفحة ");
                        x.CurrentPageNumber();
                        x.Span(" من ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("قائمة العملاء")
                        .FontSize(18).Bold().FontColor("#1e3a5f");
                    col.Item().AlignRight().Text($"تاريخ التصدير: {DateTime.Now:yyyy-MM-dd HH:mm}")
                        .FontSize(9).FontColor("#64748b");
                });
            });
        }

        private void ComposeContent(IContainer container, List<CustomerDto> customers)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(30);  // م
                    cols.RelativeColumn(2);   // رقم العميل
                    cols.RelativeColumn(3);   // الاسم
                    cols.RelativeColumn(1.5f);// النوع
                    cols.RelativeColumn(3);   // البريد
                    cols.RelativeColumn(2);   // الهاتف
                    cols.RelativeColumn(3);   // العنوان
                    cols.RelativeColumn(1.5f);// المجموعات
                    cols.RelativeColumn(2);   // التاريخ
                });

                // Header
                static IContainer HeaderCell(IContainer c) =>
                    c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                     .Background("#1e3a5f").Padding(5).AlignCenter();

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("م");
                    header.Cell().Element(HeaderCell).Text("رقم العميل");
                    header.Cell().Element(HeaderCell).Text("الاسم");
                    header.Cell().Element(HeaderCell).Text("النوع");
                    header.Cell().Element(HeaderCell).Text("البريد الإلكتروني");
                    header.Cell().Element(HeaderCell).Text("الهاتف");
                    header.Cell().Element(HeaderCell).Text("العنوان");
                    header.Cell().Element(HeaderCell).Text("المجموعات");
                    header.Cell().Element(HeaderCell).Text("تاريخ الإضافة");
                });

                // Rows
                for (int i = 0; i < customers.Count; i++)
                {
                    var c = customers[i];
                    var bg = i % 2 == 0 ? "#FFFFFF" : "#f0f4f8";

                    IContainer DataCell(IContainer cell) =>
                        cell.Background(bg).BorderBottom(0.5f).BorderColor("#dee2e6")
                            .Padding(4).AlignCenter();

                    table.Cell().Element(DataCell).Text((i + 1).ToString());
                    table.Cell().Element(DataCell).Text(c.CustomerNumber);
                    table.Cell().Element(DataCell).Text(c.Name);
                    table.Cell().Element(DataCell).Text(c.CustomerTypeDisplay);
                    table.Cell().Element(DataCell).Text(c.Email ?? "-");
                    table.Cell().Element(DataCell).Text(c.Phone ?? "-");
                    table.Cell().Element(DataCell).Text(c.Address ?? "-");
                    table.Cell().Element(DataCell).Text(c.TrapGroupCount.ToString());
                    table.Cell().Element(DataCell).Text(c.CreatedAt.ToString("yyyy-MM-dd"));
                }
            });
        }
    }
}
