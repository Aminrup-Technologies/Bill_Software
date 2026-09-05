using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;

namespace Bill_Software.corporate.business.app
{
    internal static class InvoiceListHelper
    {
        private const string ExportVersion = "v3";
        private static readonly string[] ExpectedInvoiceSources =
        {
            "Purchase Order",
            "Quotation",
            "Proforma",
            "Delivery Challan",
            "Manual"
        };

        public static string FmtDate(object v)
        {
            object parsed = ParseDate(v);
            return parsed is DateTime ? ((DateTime)parsed).ToString("dd-MMM-yyyy") : "";
        }

        public static string FmtMail(object v)
        {
            string d = FmtDate(v);
            return string.IsNullOrEmpty(d) ? "" : "<span class='badge'>Mail</span> " + d + "<br />";
        }

        public static string FmtStamp(object v)
        {
            if (v == null || v == DBNull.Value) return "";
            DateTime d;
            if (v is DateTime)
            {
                d = (DateTime)v;
                return d == DateTime.MinValue ? "" : d.ToString("dd-MMM-yyyy hh:mm tt");
            }
            string s = Convert.ToString(v);
            if (string.IsNullOrWhiteSpace(s)) return "";
            object parsed = ParseDate(s);
            if (!(parsed is DateTime)) return "";
            d = (DateTime)parsed;
            return d == DateTime.MinValue ? "" : d.ToString("dd-MMM-yyyy hh:mm tt");
        }

        public static string FormatExportDateFilter(string fromDate, string toDate)
        {
            bool hasFrom = !string.IsNullOrWhiteSpace(fromDate);
            bool hasTo = !string.IsNullOrWhiteSpace(toDate);
            if (!hasFrom && !hasTo) return "";
            if (hasFrom && hasTo) return fromDate.Trim() + " to " + toDate.Trim();
            return hasFrom ? "From " + fromDate.Trim() : "To " + toDate.Trim();
        }

        public static void PrepareInvoiceExport(DataTable dt)
        {
            ConvertColumn(dt, "Invoice Date", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Quotation Date", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Mail Date", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Delivery Date", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Created Timestamp", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Qty", typeof(double), ParseNum);
            ConvertColumn(dt, "Rate", typeof(double), ParseNum);
            ConvertColumn(dt, "Line Discount %", typeof(double), ParseNum);
            ConvertColumn(dt, "Taxable Value", typeof(double), ParseNum);
            ConvertColumn(dt, "GST %", typeof(double), ParseNum);
            ConvertColumn(dt, "Item Net Value", typeof(double), ParseNum);
            ConvertColumn(dt, "Invoice GST Amount", typeof(double), ParseNum);
            ConvertColumn(dt, "Invoice Grand Total", typeof(double), ParseNum);
            ConvertColumn(dt, "Freight", typeof(double), ParseNum);
            ConvertColumn(dt, "Other Charges", typeof(double), ParseNum);
        }

        public static void ExportXlsx(HttpResponse response, DataTable dt, string sheetName, string filename)
        {
            ExportXlsx(response, dt, sheetName, filename, null, null);
        }

        public static void ExportXlsx(HttpResponse response, DataTable dt, string sheetName, string filename, string exportSource, string dateFilter)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt, sheetName);
                ApplyInvoiceLinesSheet(ws);

                AddExportInfoSheet(wb, dt, sheetName, exportSource, dateFilter);
                AddInvoiceSourceSummarySheet(wb, dt);

                response.Clear();
                response.Buffer = true;
                response.Charset = "";
                response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                response.AddHeader("content-disposition", "attachment;filename=" + filename + ".xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(response.OutputStream);
                    response.Flush();
                    response.End();
                }
            }
        }

        private static void ApplyInvoiceLinesSheet(IXLWorksheet ws)
        {
            int lastCol = ws.LastColumnUsed().ColumnNumber();
            int lastRow = ws.LastRowUsed().RowNumber();
            var usedRange = ws.Range(1, 1, lastRow, lastCol);

            ApplyGrayHeader(ws, lastCol);
            FreezeAndFilter(ws, usedRange);

            FormatNamedColumns(ws, new[] { "Invoice Date", "Quotation Date", "Mail Date", "Delivery Date" }, "dd-MMM-yyyy");
            FormatNamedColumns(ws, new[] { "Created Timestamp" }, "dd-MMM-yyyy hh:mm tt");
            WrapNamedColumns(ws, new[] { "Item Remarks" });
            FormatNamedColumns(ws, new[] { "Rate", "Taxable Value", "Item Net Value", "Invoice GST Amount", "Invoice Grand Total", "Freight", "Other Charges" }, "#,##0.00");
            FormatNamedColumns(ws, new[] { "Qty" }, "#,##0.###");
            FormatNamedColumns(ws, new[] { "GST %", "Line Discount %" }, "0.00");

            ws.Columns().AdjustToContents();
        }

        private static void AddExportInfoSheet(XLWorkbook wb, DataTable dt, string sheetName, string exportSource, string dateFilter)
        {
            DateTime now = DateTime.Now;
            string generatedBy = "";
            if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["USERID"] != null)
                generatedBy = Convert.ToString(HttpContext.Current.Session["USERID"]);

            var rows = new[]
            {
                new[] { "Export Date", now.ToString("dd-MMM-yyyy") },
                new[] { "Export Time", now.ToString("hh:mm tt") },
                new[] { "Company Code", CompanyContext.CurrentCompanyCode },
                new[] { "Sheet Name", sheetName },
                new[] { "Export Version", ExportVersion },
                new[] { "Generated By", generatedBy ?? "" },
                new[] { "Invoice Rows", dt.Rows.Count.ToString(CultureInfo.InvariantCulture) },
                new[] { "Export Source", exportSource ?? "" },
                new[] { "Date Filter", dateFilter ?? "" }
            };

            var ws = wb.Worksheets.Add("Export_Info");
            ws.Cell(1, 1).Value = "Label";
            ws.Cell(1, 2).Value = "Value";
            ApplyGrayHeader(ws, 2);

            for (int i = 0; i < rows.Length; i++)
            {
                ws.Cell(i + 2, 1).Value = rows[i][0];
                ws.Cell(i + 2, 1).Style.Font.Bold = true;
                ws.Cell(i + 2, 2).Value = rows[i][1];
            }

            ws.Columns().AdjustToContents();
        }

        private static void AddInvoiceSourceSummarySheet(XLWorkbook wb, DataTable dt)
        {
            DataTable summary = BuildInvoiceSourceSummary(dt);
            var ws = wb.Worksheets.Add(summary, "Invoice_Source_Summary");

            int lastCol = ws.LastColumnUsed().ColumnNumber();
            int lastRow = ws.LastRowUsed().RowNumber();
            var usedRange = ws.Range(1, 1, lastRow, lastCol);

            ApplyGrayHeader(ws, lastCol);
            FreezeAndFilter(ws, usedRange);
            ws.Columns().AdjustToContents();
        }

        private static DataTable BuildInvoiceSourceSummary(DataTable lines)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string source in ExpectedInvoiceSources)
                counts[source] = 0;

            if (lines.Columns.Contains("Invoice Source"))
            {
                foreach (DataRow row in lines.Rows)
                {
                    string source = Convert.ToString(row["Invoice Source"]);
                    if (string.IsNullOrWhiteSpace(source))
                        source = "Manual";
                    if (!counts.ContainsKey(source))
                        counts[source] = 0;
                    counts[source]++;
                }
            }

            DataTable summary = new DataTable();
            summary.Columns.Add("Invoice Source", typeof(string));
            summary.Columns.Add("Invoice Count", typeof(int));

            foreach (string source in ExpectedInvoiceSources)
                summary.Rows.Add(source, counts[source]);
            foreach (var item in counts)
            {
                if (Array.IndexOf(ExpectedInvoiceSources, item.Key) < 0)
                    summary.Rows.Add(item.Key, item.Value);
            }

            return summary;
        }

        private static void ApplyGrayHeader(IXLWorksheet ws, int lastCol)
        {
            var headerRange = ws.Range(1, 1, 1, lastCol);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.WrapText = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private static void FreezeAndFilter(IXLWorksheet ws, IXLRange usedRange)
        {
            ws.SheetView.FreezeRows(1);
            if (ws.Tables.Any())
            {
                foreach (var table in ws.Tables)
                    table.ShowAutoFilter = true;
            }
            else
            {
                usedRange.SetAutoFilter();
            }
        }

        private static void FormatNamedColumns(IXLWorksheet ws, string[] names, string format)
        {
            foreach (var name in names)
            {
                var cell = ws.Row(1).CellsUsed().FirstOrDefault(c => Convert.ToString(c.Value) == name);
                if (cell == null) continue;
                ws.Column(cell.Address.ColumnNumber).Style.NumberFormat.Format = format;
            }
        }

        private static void WrapNamedColumns(IXLWorksheet ws, string[] names)
        {
            foreach (var name in names)
            {
                var cell = ws.Row(1).CellsUsed().FirstOrDefault(c => Convert.ToString(c.Value) == name);
                if (cell == null) continue;
                ws.Column(cell.Address.ColumnNumber).Style.Alignment.WrapText = true;
            }
        }

        private static void ConvertColumn(DataTable dt, string name, Type type, Func<object, object> conv)
        {
            if (!dt.Columns.Contains(name)) return;
            DataColumn old = dt.Columns[name];
            DataColumn neu = new DataColumn(name + "_n", type);
            dt.Columns.Add(neu);
            foreach (DataRow r in dt.Rows)
            {
                if (r[old] == DBNull.Value || r[old] == null)
                    r[neu] = DBNull.Value;
                else
                {
                    object parsed = conv(r[old]);
                    r[neu] = parsed ?? (object)DBNull.Value;
                }
            }
            int ord = old.Ordinal;
            dt.Columns.Remove(old);
            neu.ColumnName = name;
            neu.SetOrdinal(ord);
        }

        private static object ParseDate(object v)
        {
            DateTime d;
            string s = Convert.ToString(v);
            if (DateTime.TryParseExact(s, new[] { "dd-MMM-yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy" },
                CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out d)) return d;
            if (DateTime.TryParse(s, out d)) return d;
            return null;
        }

        private static object ParseNum(object v)
        {
            double n;
            if (double.TryParse(Convert.ToString(v), NumberStyles.Any, CultureInfo.InvariantCulture, out n)) return n;
            if (double.TryParse(Convert.ToString(v), NumberStyles.Any, CultureInfo.CurrentCulture, out n)) return n;
            return null;
        }
    }
}
