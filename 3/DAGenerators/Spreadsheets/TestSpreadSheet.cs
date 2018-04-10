using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DAGenerators.Spreadsheets
{
    public class TestSpreadSheet : SpreadsheetBase
    {
        public TestSpreadSheet()
        {
            this.ExcelPackage = new ExcelPackage();
            this.ExcelPackage.Workbook.Properties.Title = "Test Spreadsheet";

            this.ExcelPackage.Workbook.Worksheets.Add("sample worksheet");
            var ws = this.ExcelPackage.Workbook.Worksheets[1];
            ws.DefaultRowHeight = 15;
            ws.Cells.Style.Font.Name = "Calibri";
            ws.Cells.Style.Font.Size = 9;
            ws.Cells[1, 1, 10, 18].Style.Font.Size = 11;

            var col = ws.Column(1);
            col.Width = 9.140625;
            col = ws.Column(2);
            col.Width = 26.28515625;
            col.Style.Font.Bold = true;

            ws.Cells[8, 2].Value = "Weekly Summary...";
            //ws.Cells[11, 2, 11, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //ws.Cells[11, 2, 11, 18].Style.Fill.PatternColor.Tint = 0.39997558519241921M;
            //ws.Cells[11, 2, 11, 18].Style.Fill.BackgroundColor.Indexed = 64;
            ws.Cells[11, 2].Value = "Weekly";
            ws.Cells[11, 2, 11, 14].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
        }
    }
}
