using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class AmazonRowConverter
    {
        private List<List<object>> rows;
        private Dictionary<string, int> columnLookup;
        private bool includeConvMetrics;

        public AmazonRowConverter(ReportData reportData, bool includeConvMetrics)
        {
            this.rows = reportData.rows;
            this.includeConvMetrics = includeConvMetrics;
            this.columnLookup = reportData.CreateColumnLookup();
        }
        private void CheckColumnLookup(bool byCampaign = false, bool byLineItem = false, bool byBanner = false)
        {
            var columnsNeeded = new List<string> { "date", "cost", "impressions", "clicks" };
            if (includeConvMetrics)
                columnsNeeded.AddRange(new string[] { "conversions", "sales" });
            if (byCampaign)
                columnsNeeded.Add("campaign");
            if (byLineItem)
                columnsNeeded.Add("lineItem");
            if (byBanner)
                columnsNeeded.Add("banner");
            var missingColumns = columnsNeeded.Where(c => !columnLookup.ContainsKey(c));
            if (missingColumns.Any())
                throw new Exception("Missing columns in lookup: " + String.Join(", ", missingColumns));
        }

        public IEnumerable<DailySummary> EnumerateDailySummaries()
        {
            CheckColumnLookup();
            foreach (var row in rows)
            {
                var daysum = RowToDailySummary(row);
                if (daysum != null)
                    yield return daysum;
            }
        }
        public IEnumerable<StrategySummary> EnumerateStrategySummaries()
        {
            CheckColumnLookup(byCampaign: true);
            foreach (var row in rows)
            {
                var sum = RowToStrategySummary(row);
                if (sum != null)
                    yield return sum;
            }
        }
        public IEnumerable<AdSetSummary> EnumerateAdSetSummaries()
        {
            CheckColumnLookup(byLineItem: true);
            foreach (var row in rows)
            {
                var sum = RowToAdSetSummary(row);
                if (sum != null)
                    yield return sum;
            }
        }
        public IEnumerable<TDadSummary> EnumerateTDadSummaries()
        {
            CheckColumnLookup(byBanner: true);
            foreach (var row in rows)
            {
                var sum = RowToTDadSummary(row);
                if (sum != null)
                    yield return sum;
            }
        }

        public DailySummary RowToDailySummary(List<object> row)
        {
            try
            {
                var daysum = new DailySummary();
                AssignDailySummaryProperties(daysum, row);
                return daysum;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
        public StrategySummary RowToStrategySummary(List<object> row)
        {
            try
            {
                var summary = new StrategySummary
                {
                    StrategyName = Convert.ToString(row[columnLookup["campaign"]])
                };
                AssignDailySummaryProperties(summary, row);
                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
        public AdSetSummary RowToAdSetSummary(List<object> row)
        {
            try
            {
                var summary = new AdSetSummary
                {
                    AdSetName = Convert.ToString(row[columnLookup["lineItem"]])
                };
                AssignDailySummaryProperties(summary, row);
                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
        public TDadSummary RowToTDadSummary(List<object> row)
        {
            try
            {
                var summary = new TDadSummary
                {
                    TDadName = Convert.ToString(row[columnLookup["banner"]])
                };
                AssignDailySummaryProperties(summary, row);
                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        private void AssignDailySummaryProperties(DatedStatsSummary summary, List<object> row)
        {
            summary.Date = DateTime.Parse(row[columnLookup["date"]].ToString());
            summary.Cost = Convert.ToDecimal(row[columnLookup["cost"]]);
            summary.Impressions = Convert.ToInt32(row[columnLookup["impressions"]]);
            summary.Clicks = Convert.ToInt32(row[columnLookup["clicks"]]);
            if (includeConvMetrics)
            {
                summary.PostViewConv = Convert.ToInt32(row[columnLookup["conversions"]]);
                if (summary is DatedStatsSummaryWithRev)
                    ((DatedStatsSummaryWithRev)summary).PostViewRev = Convert.ToDecimal(row[columnLookup["sales"]]);
            }
        }
    }

    public class AmazonTransformer
    {
        private List<List<object>> rows;
        private Dictionary<string, int> columnLookup;
        private bool includeBasicStats;
        private bool includeConvStats;
        private bool includeAdInteractionType;
        private bool includeCampaign;
        private bool includeLineItem;
        private bool includeBanner;

        public AmazonTransformer(ReportData reportData, bool basicStatsOnly = false, bool convStatsOnly = false, bool byCampaign = false, bool byLineItem = false, bool byBanner = false)
        {
            this.rows = reportData.rows;
            this.columnLookup = reportData.CreateColumnLookup();

            this.includeBasicStats = this.includeConvStats = this.includeAdInteractionType = true;
            if (basicStatsOnly)
                this.includeConvStats = this.includeAdInteractionType = false;
            if (convStatsOnly)
                this.includeBasicStats = false;

            this.includeCampaign = byCampaign;
            this.includeLineItem = byLineItem;
            this.includeBanner = byBanner;
        }
        public IEnumerable<AmazonSummary> EnumerateAmazonSummaries()
        {
            foreach (var row in rows)
            {
                var summary = RowToAmazonSummary(row);
                if (summary != null)
                    yield return summary;
            }
        }

        private AmazonSummary RowToAmazonSummary(List<object> row)
        {
            try
            {
                var summary = new AmazonSummary();
                AssignSummaryProperties(summary, row);
                return summary;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
        private void AssignSummaryProperties(AmazonSummary summary, List<object> row)
        {
            summary.Date = DateTime.Parse(row[columnLookup["date"]].ToString());
            if (includeBasicStats)
            {
                summary.Cost = Convert.ToDecimal(row[columnLookup["cost"]]);
                summary.Impressions = Convert.ToInt32(row[columnLookup["impressions"]]);
                summary.Clicks = Convert.ToInt32(row[columnLookup["clicks"]]);
            }
            if (includeConvStats)
            {
                summary.Conversions = Convert.ToInt32(row[columnLookup["conversions"]]);
                summary.Sales = Convert.ToDecimal(row[columnLookup["sales"]]);
            }
            if (includeAdInteractionType)
            {
                summary.AdInteractionType = Convert.ToString(row[columnLookup["adInteractionType"]]);
                if (summary.AdInteractionType.StartsWith("Recent ")) // "Recent Impresson", "Recent Click": strip off "Recent "
                    summary.AdInteractionType = summary.AdInteractionType.Substring(7);
            }
            if (includeCampaign)
                summary.Campaign = Convert.ToString(row[columnLookup["campaign"]]);
            if (includeLineItem)
                summary.LineItem = Convert.ToString(row[columnLookup["lineItem"]]);
            if (includeBanner)
                summary.Banner = Convert.ToString(row[columnLookup["banner"]]);
        }
    }
}
