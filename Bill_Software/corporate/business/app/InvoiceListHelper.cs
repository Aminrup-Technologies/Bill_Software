using System;
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

        public static void PrepareInvoiceExport(DataTable dt)
        {
            ConvertColumn(dt, "Invoice Date", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Quotation Date", typeof(DateTime), ParseDate);
            ConvertColumn(dt, "Mail Date", typeof(DateTime), ParseDate);
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
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt, sheetName);

                int lastCol = ws.LastColumnUsed().ColumnNumber();
                int lastRow = ws.LastRowUsed().RowNumber();
                var usedRange = ws.Range(1, 1, lastRow, lastCol);

                var headerRange = ws.Range(1, 1, 1, lastCol);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

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

                FormatNamedColumns(ws, new[] { "Invoice Date", "Quotation Date", "Mail Date" }, "dd-MMM-yyyy");
                FormatNamedColumns(ws, new[] { "Created Timestamp" }, "dd-MMM-yyyy hh:mm tt");
                FormatNamedColumns(ws, new[] { "Rate", "Taxable Value", "Item Net Value", "Invoice GST Amount", "Invoice Grand Total", "Freight", "Other Charges" }, "#,##0.00");
                FormatNamedColumns(ws, new[] { "Qty" }, "#,##0.###");
                FormatNamedColumns(ws, new[] { "GST %", "Line Discount %" }, "0.00");

                ws.Columns().AdjustToContents();

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

        private static void FormatNamedColumns(IXLWorksheet ws, string[] names, string format)
        {
            foreach (var name in names)
            {
                var cell = ws.Row(1).CellsUsed().FirstOrDefault(c => Convert.ToString(c.Value) == name);
                if (cell == null) continue;
                ws.Column(cell.Address.ColumnNumber).Style.NumberFormat.Format = format;
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
