using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;

namespace DAGenerators.Spreadsheets
{
    public static class SpreadsheetExtensions
    {
        public static void InsertRowZ(this ExcelWorksheet workSheet, int rowStart, int count, int copyStylesFromRow)
        {
            workSheet.InsertRow(rowStart, count, copyStylesFromRow);

            UpdateRangesForRowInsert(workSheet, rowStart, count);
            UpdateDrawingsForRowInsertDelete(workSheet, rowStart, count);
        }

        public static void DeleteRowZ(this ExcelWorksheet workSheet, int rowFrom, int rows)
        {
            workSheet.DeleteRow(rowFrom, rows);

            //UpdateRangesForRowInsertDelete(workSheet, rowFrom, -rows); // TODO
            UpdateDrawingsForRowInsertDelete(workSheet, rowFrom, -rows);
        }

        private static void UpdateDrawingsForRowInsertDelete(this ExcelWorksheet workSheet, int rowStart, int count)
        {
            foreach (ExcelDrawing drawing in workSheet.Drawings)
            {
                if (drawing.From.Row + 1 >= rowStart) // From.Row is 0-based, so add one for the comparison
                {
                    drawing.SetPosition(drawing.From.Row + count, drawing.From.RowOff, drawing.From.Column, drawing.From.ColumnOff);
                    // NOTE: doesn't work well when RowOff or ColumnOff are not zero
                }
            }
        }

        // TODO: also update ranges for RowDelete (when count is negative)
        // From: https://epplus.codeplex.com/workitem/13628
        private static void UpdateRangesForRowInsert(this ExcelWorksheet workSheet, int rowStart, int count)
        {
            // key: NamedRange, value: Is named range encompassing inserted rows
            var rangesToUpdate = new Dictionary<ExcelNamedRange, bool>();
            foreach (ExcelNamedRange range in workSheet.Names)
            {
                try
                {
                    if (range.End.Row >= rowStart)
                        rangesToUpdate.Add(range, range.Start.Row < rowStart);
                }
                catch
                {
                    /* Excel Defined Names may contain other refersTo values than ranges.
                     * We can skip those. */
                }
            }
            foreach (var range in rangesToUpdate)
            {
                workSheet.Names.Remove(range.Key.Name);
                ExcelRangeBase newRange;
                if (range.Value)
                    newRange = workSheet.Cells[range.Key.Start.Row, range.Key.Start.Column, range.Key.End.Row + count, range.Key.End.Column];
                else
                    newRange = range.Key.Offset(count, 0);

                var newNamedRange = workSheet.Names.Add(range.Key.Name, newRange);
                newNamedRange.NameComment = range.Key.NameComment;
                newNamedRange.IsNameHidden = range.Key.IsNameHidden;
            }
        }
        public static void InsertRowY(this ExcelWorksheet workSheet, int rowStart, int count, int copyStylesFromRow)
        {
            workSheet.InsertRow(rowStart, count, copyStylesFromRow);
            foreach (var name in workSheet.Workbook.Names)
            {
                var x = name;
            }

            var ng = (from item in workSheet.Workbook.Names
                      where item.Worksheet.Name.ToUpper() == workSheet.Name.ToUpper() &&
                            item.Address.ToUpper().Contains(workSheet.Name.ToUpper()) &&
                            item.Start.Row >= rowStart
                      select item).ToList();

            for (int i = 0; i < ng.Count(); i++)
            {
                workSheet.Workbook.Names.Remove(ng[i].Name);
                var newitem = ng[i].Offset(count, 0);
                workSheet.Workbook.Names.Add(ng[i].Name, newitem);
            }
        }

        // From: http://stackoverflow.com/questions/25623324/epplus-how-to-change-colors-of-pie-chart-in-excel
        public static void SetSeriesStyle(this ExcelChart chart, ExcelChartSerie series, Color color, decimal? thickness = null, int iOffset = 0)
        {
            if (thickness < 0) throw new ArgumentOutOfRangeException("thickness");
            var i = iOffset;
            var found = false;
            foreach (var s in chart.Series)
            {
                if (s == series)
                {
                    found = true;
                    break;
                }
                ++i;
            }
            if (!found) throw new InvalidOperationException("series not found.");
            //Get the nodes
            var nsm = chart.WorkSheet.Drawings.NameSpaceManager;
            var nschart = nsm.LookupNamespace("c");
            var nsa = nsm.LookupNamespace("a");
            var node = chart.ChartXml.SelectSingleNode(@"c:chartSpace/c:chart/c:plotArea/c:lineChart/c:ser[c:idx[@val='" + i.ToString(CultureInfo.InvariantCulture) + "']]", nsm);
            //if (node == null)
            //    node = chart.ChartXml.SelectSingleNode(@"c:chartSpace/c:chart/c:plotArea/c:barChart/c:ser[c:idx[@val='" + i.ToString(CultureInfo.InvariantCulture) + "']]", nsm);
            var doc = chart.ChartXml;

            //Add the solid fill node
            var spPr = doc.CreateElement("c:spPr", nschart);
            var ln = spPr.AppendChild(doc.CreateElement("a:ln", nsa));
            if (thickness.HasValue)
            {
                var w = ln.Attributes.Append(doc.CreateAttribute("w"));
                w.Value = Math.Round(thickness.Value * 12700).ToString(CultureInfo.InvariantCulture);
                var cap = ln.Attributes.Append(doc.CreateAttribute("cap"));
                cap.Value = "rnd";
            }
            var solidFill = ln.AppendChild(doc.CreateElement("a:solidFill", nsa));
            var srgbClr = solidFill.AppendChild(doc.CreateElement("a:srgbClr", nsa));
            var valattrib = srgbClr.Attributes.Append(doc.CreateAttribute("val"));

            //Set the color
            valattrib.Value = color.ToHex().Substring(1);
            node.AppendChild(spPr);
        }
        public static String ToHex(this Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
    }
}
