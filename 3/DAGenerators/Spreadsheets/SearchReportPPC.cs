using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace DAGenerators.Spreadsheets
{
    //TODO: make a SearchReportECom (or ...Retail) that inherits from this
    public class SearchReportPPC : SpreadsheetBase
    {
        private const int Row_SummaryDate = 8;
        //private const int Row_StatsHeader = 11;
        private const int Row_ClientNameBottom = 30;
        private const int Row_Charts = 33;

        private const int Col_LeftChart = 2;
        private const int Col_RightChart = 9;
        private const int ChartWidth = 590;
        private const int ChartHeight = 280;

        protected string TemplateFilename = "SearchPPCtemplate.xlsx";
        private const int NumRows_SectionHeader = 2;

        private const int StartRow_Weekly = 12;
        private const int StartRow_Monthly = 16;
        protected bool WeeklyFirst
        {
            get { return StartRow_Weekly <= StartRow_Monthly; }
        }
        protected int StartRow_YoYSummary = 20;

        protected int StartRow_WoWSummary = 26;

        private const int StartRow_WeeklyCampaignPerfTemplate = 51;
        private const int StartRow_MonthlyCampaignPerfTemplate = 57;
        private const int NumRows_CampaignPerfTemplate = 3;
        private const int NumRows_CampaignPerfSubTemplate = 2;

        private ExcelWorksheet WS1 { get { return this.ExcelPackage.Workbook.Worksheets[1]; } }
        public Sheet1Metrics Metrics1 = new Sheet1Metrics();
        private ExcelWorksheet WS2 { get { return this.ExcelPackage.Workbook.Worksheets[2]; } }
        public Sheet2Metrics Metrics2 = new Sheet2Metrics();
        private ExcelWorksheet WS3 { get { return this.ExcelPackage.Workbook.Worksheets[3]; } }
        // Metrics1 also used for WS3

        protected int NumWeeksAdded { get; set; }
        protected int NumMonthsAdded { get; set; }
        protected int NumWeekRowsAdded { get; set; }  // not including template rows
        protected int NumMonthRowsAdded { get; set; } // not including template rows

        protected int NumCampaignPerfWeeksAdded { get; set; }
        protected int NumCampaignPerfMonthsAdded { get; set; }

        // --- Sheet 2 ---
        private const int StartRow_YoYFull = 12;
        private const int Row_YoYCharts = 15;
        protected int NumYoYMonthsAdded { get; set; }
        protected int NumYoYMonthRowsAdded { get; set; }

        // --- Sheet 3 ---
        private const int StartRow_DisplayWeeks = 4;
        private const int StartRow_SearchWeeks = 8;
        protected int NumDisplayWeeksAdded { get; set; }
        protected int NumSearchWeeksAdded { get; set; }


        public SearchReportPPC()
        {
        }

        public void Setup(string templateFolder)
        {
            Setup();
            var fileInfo = new FileInfo(Path.Combine(templateFolder, TemplateFilename));
            this.ExcelPackage = new ExcelPackage(fileInfo);

            SetReportDate(DateTime.Today);
            //SetColumnHeaders(); // do this after specifying which metrics are "shown"
        }

        protected virtual void Setup()
        {
            //TODO: a way for the caller to specify different property names
            Metrics1.Title = new Metric(2, null) { MemberName = "Title" };
            Metrics1.Clicks = new Metric(3, "Clicks") { MemberName = "Clicks_" };
            Metrics1.Impressions = new Metric(4, "Impressions") { MemberName = "Impressions_" };
            Metrics1.CTR = new Metric(5, "CTR");
            Metrics1.Cost = new Metric(6, "Spend") { MemberName = "Cost_" };
            Metrics1.CPC = new Metric(7, "CPC");
            Metrics1.Orders = new Metric(8, "Orders") { MemberName = "Orders_" };
            Metrics1.Revenue = new Metric(9, "Revenue") { MemberName = "Revenue_" };
            Metrics1.Net = new Metric(10, "Net");
            Metrics1.ViewThrus = new Metric(11, "ViewThrus") { MemberName = "ViewThrus_" };
            Metrics1.ViewThruRev = new Metric(12, "ViewThruRev");
            Metrics1.CassConvs = new Metric(13, "ClickAssistConv") { MemberName = "CassConvs_" };
            Metrics1.CassConVal = new Metric(14, "CAC Val") { MemberName = "CassConVal_" };
            Metrics1.CPO = new Metric(15, "Cost/Order");
            Metrics1.OrderRate = new Metric(16, "Order Rate");
            Metrics1.RevPerOrder = new Metric(17, "Rev/Order");
            Metrics1.ROAS = new Metric(18, "ROAS");

            Metrics2.Title = new Metric(2, null) { MemberName = "Title" };
            //Metrics2.Clicks1 = new Metric(3, "ClicksYr1") { MemberName = "PrevClicks" };
            //Metrics2.Clicks2 = new Metric(4, "ClicksYr2") { MemberName = "Clicks_" };
            //Metrics2.Impressions1 = new Metric(5, "ImpressionsYr1") { MemberName = "PrevImpressions" };
            //Metrics2.Impressions2 = new Metric(6, "ImpressionsYr2") { MemberName = "Impressions_" };
            Metrics2.Cost1 = new Metric(3, "SpendYr1") { MemberName = "PrevCost" };
            Metrics2.Cost2 = new Metric(4, "SpendYr2") { MemberName = "Cost_" };
            Metrics2.Orders1 = new Metric(5, "OrdersYr1") { MemberName = "PrevOrders" };
            Metrics2.Orders2 = new Metric(6, "OrdersYr2") { MemberName = "Orders_" };
            Metrics2.Revenue1 = new Metric(7, "RevenueYr1") { MemberName = "PrevRevenue" };
            Metrics2.Revenue2 = new Metric(8, "RevenueYr2") { MemberName = "Revenue_" };
            Metrics2.CPO1 = new Metric(9, "CPO_Yr1");
            Metrics2.CPO2 = new Metric(10, "CPO_Yr2");
            Metrics2.ROAS1 = new Metric(11, "ROAS_Yr1");
            Metrics2.ROAS2 = new Metric(12, "ROAS_Yr2");
        }

        // Do this after setting up the columns
        public void MakeColumnHidden(Metric metric)
        {
            if (metric != null)
            {
                WS1.Column(metric.ColNum).Hidden = true;
                WS3.Column(metric.ColNum).Hidden = true;
            }
            //Note: the column will still exist in the spreadsheet; it will be hidden... the user can unhide it
        }

        public virtual void SetReportDate(DateTime date)
        {
            WS1.Cells[Row_SummaryDate, 2].Value = "Report Summary - through " + date.ToShortDateString();
        }
        public virtual void SetReportingPeriod(DateTime fromDate, DateTime toDate)
        {
        }

        //public void SetColumnHeaders()
        //{
        //    var metrics = GetMetrics(true);
        //    foreach (var metric in metrics)
        //    {
        //        WS.Cells[Row_StatsHeader, metric.ColNum].Value = metric.DisplayName;
        //    }
        //    // Q: What about the headers under Monthly, YoY, Weekly Perf...?
        //}

        public virtual void SetClientName(string clientName)
        {
            WS1.Cells[Row_ClientNameBottom + NumWeekRowsAdded + NumMonthRowsAdded, 2].Value = "Direct Agents | " + clientName.ToUpper();
        }

        public void LoadWeeklyStats<T>(IEnumerable<T> stats)
        {
            int startRow = StartRow_Weekly + (WeeklyFirst ? 0 : NumMonthRowsAdded);
            NumWeekRowsAdded = LoadStats(Metrics1, WS1, startRow, stats);
            NumWeeksAdded = stats.Count();
            if (NumWeeksAdded == 0)
            {   // Delete the entire section, including blank row above
                int rowsToDelete = NumRows_SectionHeader + 1;
                WS1.DeleteRowZ(startRow - rowsToDelete, rowsToDelete);
                NumWeekRowsAdded -= rowsToDelete;
            }
        }
        public void LoadMonthlyStats<T>(IEnumerable<T> stats)
        {
            int startRow = StartRow_Monthly + (WeeklyFirst ? NumWeekRowsAdded : 0);
            NumMonthRowsAdded = LoadStats(Metrics1, WS1, startRow, stats);
            NumMonthsAdded = stats.Count();
            if (NumMonthsAdded == 0)
            {   // Delete the entire section, including blank row above
                int rowsToDelete = NumRows_SectionHeader + 1;
                WS1.DeleteRowZ(startRow - rowsToDelete, rowsToDelete);
                NumMonthRowsAdded -= rowsToDelete;
            }
        }

        public void LoadWeeklyDisplayStats<T>(IEnumerable<T> stats)
        {
            NumDisplayWeeksAdded = LoadStats(Metrics1, WS3, StartRow_DisplayWeeks, stats);
        }
        public void LoadWeeklySearchStats<T>(IEnumerable<T> stats)
        {
            int startRow = StartRow_SearchWeeks + NumDisplayWeeksAdded;
            NumSearchWeeksAdded = LoadStats(Metrics1, WS3, startRow, stats);
        }

        // (for the most recently completed month)
        // IEnumerable<T> stats should have two elements: last year's and this year's stats
        public virtual void LoadYearOverYear_Summary<T>(IEnumerable<T> stats)
        {
            LoadXOverX_Summary(stats, StartRow_YoYSummary);
        }

        // IEnumerable<T> stats should have two elements: last week's and this week's stats
        public virtual void LoadWeekOverWeek_Summary<T>(IEnumerable<T> stats)
        {
            LoadXOverX_Summary(stats, StartRow_WoWSummary);
        }

        // IEnumerable<T> stats should have two elements: last week's and this week's stats
        public virtual void LoadXOverX_Summary<T>(IEnumerable<T> stats, int sectionStartRow)
        {
            int startRow = sectionStartRow + NumWeekRowsAdded + NumMonthRowsAdded;
            int blankRows = 2;
            LoadStats(Metrics1, WS1, startRow, stats, blankRows);

            //XoX diff row...
            string diffFormula = "IFERROR((R[-1]C-R[-2]C)/R[-2]C,\"-\")";
            var metrics = Metrics1.GetAll(false);
            foreach (var metric in metrics)
            {
                WS1.Cells[startRow + 2, metric.ColNum].FormulaR1C1 = diffFormula;
            }
        }

        // Note: Load monthly CampaignPerfStats first, then weekly.
        // Load weeks/months in this order: latest, second-latest, etc...
        public void LoadMonthlyCampaignPerfStats<T>(Dictionary<string, IEnumerable<T>> campaignStatsDict, bool collapse, DateTime monthStart, DateTime monthEnd)
        {
            int startRowTemplate = StartRow_MonthlyCampaignPerfTemplate + NumWeekRowsAdded + NumMonthRowsAdded;
            LoadCampaignPerfStats<T>(campaignStatsDict, collapse, monthStart, monthEnd, startRowTemplate, true);
        }
        public void LoadWeeklyCampaignPerfStats<T>(Dictionary<string, IEnumerable<T>> campaignStatsDict, bool collapse, DateTime weekStart, DateTime weekEnd)
        {
            int startRowTemplate = StartRow_WeeklyCampaignPerfTemplate + NumWeekRowsAdded + NumMonthRowsAdded;
            LoadCampaignPerfStats<T>(campaignStatsDict, collapse, weekStart, weekEnd, startRowTemplate, false);
        }
        // Load stats for one week/month (with subcategories: channels/searchaccounts)
        // campaignStatsDict should have one key for each Channel/SearchAccount
        public void LoadCampaignPerfStats<T>(Dictionary<string, IEnumerable<T>> campaignStatsDict, bool collapse, DateTime startDate, DateTime endDate,
                                             int startRowTemplate, bool monthlyNotWeekly)
        {
            int startRowStats = startRowTemplate + NumRows_CampaignPerfTemplate; // start below the template
            int numRowsAdded = 0;
            var nonComputedMetrics = Metrics1.GetNonComputed(false);
            string sumFormula;

            // Insert rows (below the template)
            WS1.InsertRowZ(startRowStats, NumRows_CampaignPerfTemplate, startRowTemplate);

            // Copy template rows (and paste just below them)
            WS1.Cells[startRowTemplate + ":" + (startRowTemplate + NumRows_CampaignPerfTemplate - 1)]
                .Copy(WS1.Cells[startRowStats + ":" + (startRowStats + NumRows_CampaignPerfTemplate - 1)]);

            // Now we use the "copy" to fill in stats for this week/month. The template remains above.

            // Populate the rows for each Subgroup (Channel/SearchAccount)...
            var subgroupSummaryOffsets = new List<int>(); // used to generate grand total sums
            int totalSubgroupRows = 0;
            int startRowSubgroupTemplate = startRowStats;
            int startRowSubgroupStats = startRowSubgroupTemplate + NumRows_CampaignPerfSubTemplate; // (below the template)
            var subgroupKeys = campaignStatsDict.Keys.Where(k => k != "Total").OrderBy(k => k == "Google").ThenByDescending(k => k);
            foreach (string subgroupKey in subgroupKeys) // in reverse order (so we end up with Google, then others alphabetically)
            {
                int numCampaigns = campaignStatsDict[subgroupKey].Count();
                bool lastSubgroup = (subgroupKey == subgroupKeys.Last());

                if (lastSubgroup)
                {
                    // use the template for the last subgroup
                    startRowSubgroupStats = startRowSubgroupTemplate;
                }
                else
                {
                    // insert rows and make a copy of the template
                    WS1.InsertRowZ(startRowSubgroupStats, NumRows_CampaignPerfSubTemplate, startRowSubgroupTemplate);
                    numRowsAdded += NumRows_CampaignPerfSubTemplate;

                    WS1.Cells[startRowSubgroupTemplate + ":" + (startRowSubgroupTemplate + NumRows_CampaignPerfSubTemplate - 1)]
                        .Copy(WS1.Cells[startRowSubgroupStats + ":" + (startRowSubgroupStats + NumRows_CampaignPerfSubTemplate - 1)]);
                }

                // populate the campaign rows for this subgroup
                numRowsAdded += LoadStats(Metrics1, WS1, startRowSubgroupStats, campaignStatsDict[subgroupKey], NumRows_CampaignPerfSubTemplate - 1);

                // set the subgroup title
                int subgroupTotalRow = startRowSubgroupStats + numCampaigns;
                WS1.Cells[subgroupTotalRow, 2].Value = subgroupKey;

                // fill in subgroup sum formulas
                sumFormula = String.Format("SUM(R[{0}]C:R[{1}]C)", -numCampaigns, -1);
                foreach (var metric in nonComputedMetrics)
                {
                    WS1.Cells[subgroupTotalRow, metric.ColNum].FormulaR1C1 = sumFormula;
                }

                subgroupSummaryOffsets.Add(-1 - totalSubgroupRows);
                totalSubgroupRows += numCampaigns + 1; // add one for the subgroup summary row
            }

            // Populate grand total row
            string grandTotalLabel;
            if (monthlyNotWeekly)
            {
                DateTime fullMonthEnd = startDate.AddMonths(1).AddDays(-1);
                string extra = (endDate == fullMonthEnd) ? "" : " (MTD)";
                grandTotalLabel = startDate.ToString("MMMM yyyy") + " TOTALS" + extra;
            }
            else
            {
                grandTotalLabel = String.Format("{0:M/d} - {1:M/d} TOTALS", startDate, endDate);
            }
            int grandTotalRow = startRowStats + totalSubgroupRows;
            WS1.Cells[grandTotalRow, 2].Value = grandTotalLabel;

            // grand total row sums
            subgroupSummaryOffsets.Reverse();
            sumFormula = "SUM(" + String.Join(",", subgroupSummaryOffsets.Select(offset => String.Format("R[{0}]C", offset))) + ")";
            foreach (var metric in nonComputedMetrics)
            {
                WS1.Cells[grandTotalRow, metric.ColNum].FormulaR1C1 = sumFormula;
            }

            // make rows collapsible
            for (int iRow = startRowStats; iRow < grandTotalRow; iRow++)
            {
                WS1.Row(iRow).OutlineLevel = 1;
            }
            if (collapse)
            {
                for (int iRow = startRowStats; iRow < grandTotalRow; iRow++)
                    WS1.Row(iRow).Collapsed = true;
            }

            if (monthlyNotWeekly)
                NumCampaignPerfMonthsAdded++;
            else
                NumCampaignPerfWeeksAdded++;
        }

        public void CampaignPerfStatsCleanup(bool monthlyNotWeekly)
        {
            int startRowTemplate = (monthlyNotWeekly ? StartRow_MonthlyCampaignPerfTemplate : StartRow_WeeklyCampaignPerfTemplate)
                    + NumWeekRowsAdded + NumMonthRowsAdded;
            WS1.DeleteRowZ(startRowTemplate, NumRows_CampaignPerfTemplate);

            bool deleteSection = monthlyNotWeekly ? (NumCampaignPerfMonthsAdded == 0) : (NumCampaignPerfWeeksAdded == 0);
            if (deleteSection)
            {   // Delete the entire section, including blank row above
                int rowsToDelete = NumRows_SectionHeader + 1;
                WS1.DeleteRowZ(startRowTemplate - rowsToDelete, rowsToDelete);
            }
            // TODO? Update drawings and ranges - like in InsertRowZ() ?
        }

        public virtual void CreateCharts(bool weeklyNotMonthly)
        {
            CreateChart(Metrics1.Title.ColNum, Metrics1.Revenue, Metrics1.ROAS, false, weeklyNotMonthly);
            CreateChart(Metrics1.Title.ColNum, Metrics1.Orders, Metrics1.CPO, true, weeklyNotMonthly);
        }
        protected void CreateChart(int titleCol, Metric metric1, Metric metric2, bool rightSide, bool weeklyNotMonthly)
        {
            int topRow = Row_Charts + NumWeekRowsAdded + NumMonthRowsAdded - 1;
            int leftCol = (rightSide ? Col_RightChart : Col_LeftChart) - 1;
            string chartNameSuffix = (rightSide ? "Right" : "Left");

            int startRow_Stats, numRows_Stats;
            string typeWM;
            if (weeklyNotMonthly)
            {
                startRow_Stats = StartRow_Weekly + (WeeklyFirst ? 0 : NumMonthRowsAdded);
                numRows_Stats = NumWeeksAdded;
                typeWM = "Weekly";
            }
            else
            {
                startRow_Stats = StartRow_Monthly + (WeeklyFirst ? NumWeekRowsAdded : 0);
                numRows_Stats = NumMonthsAdded;
                typeWM = "Monthly";
            }
            CreateChart(WS1, titleCol, metric1, metric2, startRow_Stats, numRows_Stats, topRow, leftCol, ChartWidth, ChartHeight, typeWM, chartNameSuffix);
        }

        public void LoadYearOverYear_Full<T>(IEnumerable<T> stats)
        {
            if (stats.Count() > 0)
            {
                NumYoYMonthRowsAdded = LoadStats(Metrics2, WS2, StartRow_YoYFull, stats);
                NumYoYMonthsAdded = stats.Count();

                CreateYearOverYear_Charts();
            }
        }

        public virtual void CreateYearOverYear_Charts()
        {
            CreateChart2_YOY(Metrics2.Title.ColNum, Metrics2.Revenue1, Metrics2.Revenue2, Metrics2.ROAS1, Metrics2.ROAS2, false, "YoY Revenue vs. ROAS");
            CreateChart2_YOY(Metrics2.Title.ColNum, Metrics2.Orders1, Metrics2.Orders2, Metrics2.CPO1, Metrics2.CPO2, true, "YoY Orders vs. CPO");
        }
        protected void CreateChart2_YOY(int titleCol, Metric metricBar1, Metric metricBar2, Metric metricLine1, Metric metricLine2, bool rightSide, string chartTitle)
        {
            int topRow = Row_YoYCharts + NumYoYMonthRowsAdded - 1;
            int leftCol = (rightSide ? Col_RightChart : Col_LeftChart) - 1;
            string chartName = "yoy" + (rightSide ? "Right" : "Left");
            CreateChart2(WS2, titleCol, metricBar1, metricBar2, metricLine1, metricLine2, StartRow_YoYFull, NumYoYMonthsAdded, topRow, leftCol, ChartWidth, ChartHeight, chartTitle, chartName);
        }

    }

    public class Sheet1Metrics : MetricsHolderBase
    {
        // Non-computed...
        public Metric Impressions, Clicks, Orders, Cost, Revenue, Calls, ViewThrus, CassConvs, CassConVal;

        // Computed...
        public Metric OrderRate, Net, RevPerOrder, CTR, CPC, CPO, ROAS, ROI, TotalLeads, CPL, ViewThruRev;

        public override IEnumerable<Metric> GetNonComputed()
        {
            var metrics = new Metric[]
            {
                Impressions, Clicks, Orders, Cost, Revenue, Calls, ViewThrus, CassConvs, CassConVal
            };
            return metrics.Where(m => m != null);
        }
        public override IEnumerable<Metric> GetComputed()
        {
            var metrics = new Metric[]
            {
                OrderRate, Net, RevPerOrder, CTR, CPC, CPO, ROAS, ROI, TotalLeads, CPL, ViewThruRev
            };
            return metrics.Where(m => m != null);
        }
    }

    public class Sheet2Metrics : MetricsHolderBase
    {
        // Non-computed...
        public Metric Impressions1, Impressions2, Clicks1, Clicks2, Orders1, Orders2, Cost1, Cost2, Revenue1, Revenue2, Calls1, Calls2;

        // Computed...
        public Metric CTR1, CTR2, CPC1, CPC2, CPO1, CPO2, ROAS1, ROAS2, TotalLeads1, TotalLeads2, CPL1, CPL2;

        public override IEnumerable<Metric> GetNonComputed()
        {
            var metrics = new Metric[]
            {
                Impressions1, Impressions2, Clicks1, Clicks2, Orders1, Orders2, Cost1, Cost2, Revenue1, Revenue2, Calls1, Calls2, TotalLeads1, TotalLeads2
            };
            return metrics.Where(m => m != null);
        }
        public override IEnumerable<Metric> GetComputed()
        {
            var metrics = new Metric[] { CTR1, CTR2, CPC1, CPC2, CPO1, CPO2, ROAS1, ROAS2, CPL1, CPL2 };
            return metrics.Where(m => m != null);
        }
    }
}
