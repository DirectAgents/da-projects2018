using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CakeExtracter.Common;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using Criteo;
using Criteo.CriteoAPI;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public abstract class CriteoApiExtracter<T> : Extracter<T>
        where T : DatedStatsSummary
    {
        protected readonly string accountCode;
        protected readonly DateRange dateRange;
        protected readonly int timezoneOffset;

        protected CriteoUtility _criteoUtility;
        //TODO: make it so that within the criteoUtility, it doesn't need to open and close the service for every call

        public CriteoApiExtracter(CriteoUtility criteoUtility, string accountCode, DateRange dateRange, int timezoneOffset = 0)
        {
            this.accountCode = accountCode;
            this.dateRange = dateRange;
            this.timezoneOffset = timezoneOffset;
            if (criteoUtility != null)
            {
                this._criteoUtility = criteoUtility;
            }
            else
            {
                _criteoUtility = new CriteoUtility(m => Logger.Info(m), m => Logger.Warn(m));
                _criteoUtility.SetCredentials(accountCode);
            }
        }

        protected IEnumerable<StrategySummary> EnumerateXmlReportRows(string reportUrl)
        {
            var rows = CriteoApiExtracter.EnumerateCriteoXmlReportRows(reportUrl);
            foreach (var row in rows)
            {
                var sum = new StrategySummary
                {
                    StrategyEid = row["campaignID"],
                    Date = DateTime.Parse(row["dateTime"]).AddHours(timezoneOffset).Date,
                    Cost = decimal.Parse(row["cost"]),
                    Impressions = int.Parse(row["impressions"]),
                    Clicks = int.Parse(row["click"]),
                    PostClickConv = int.Parse(row["sales"]),
                    PostClickRev = decimal.Parse(row["orderValue"]),
                };
                yield return sum;
            }
        }

        //public campaign[] GetCampaigns()
        //{
        //    return _criteoUtility.GetCampaigns();
        //}
    }

    public class CriteoStrategySummaryExtracter : CriteoApiExtracter<StrategySummary>
    {
        public CriteoStrategySummaryExtracter(CriteoUtility criteoUtility, string accountCode, DateRange dateRange, int timezoneOffset)
            : base(criteoUtility, accountCode, dateRange, timezoneOffset)
        { }

        protected override void Extract()
        {
            Logger.Info("Extracting StrategySummaries from Criteo API for {0} from {1:d} to {2:d}",
                        this.accountCode, this.dateRange.FromDate, this.dateRange.ToDate);
            if (this.timezoneOffset == 0)
                Extract_Daily();
            else
                Extract_Hourly();
        }

        private void Extract_Daily()
        {
            var reportUrl = _criteoUtility.GetCampaignReport(dateRange.FromDate, dateRange.ToDate);
            var reportRows = EnumerateXmlReportRows(reportUrl);
            Add(reportRows);
            End();
        }

        private void Extract_Hourly()
        {
            var adjustedBeginDate = this.dateRange.FromDate;
            var adjustedEndDate = this.dateRange.ToDate;
            if (this.timezoneOffset < 0)
                adjustedEndDate = adjustedEndDate.AddDays(1);
            else if (this.timezoneOffset > 0)
                adjustedBeginDate = adjustedBeginDate.AddDays(-1);

            var reportUrl = _criteoUtility.GetCampaignReport(adjustedBeginDate, adjustedEndDate, hourly: true);

            var dailySummaries = EnumerateRows_Hourly(reportUrl);
            Add(dailySummaries);
            End();
        }
        private IEnumerable<StrategySummary> EnumerateRows_Hourly(string reportUrl)
        {
            IEnumerable<StrategySummary> hourlySummaries = EnumerateXmlReportRows(reportUrl).ToList();
            hourlySummaries = hourlySummaries.Where(s => s.Date >= this.dateRange.FromDate && s.Date <= this.dateRange.ToDate);
            var dailyGroups = hourlySummaries.GroupBy(s => new { s.StrategyEid, s.Date });
            foreach (var group in dailyGroups)
            {
                var sum = new StrategySummary
                {
                    StrategyEid = group.Key.StrategyEid,
                    Date = group.Key.Date
                };
                sum.SetStats(group);
                yield return sum;
            }
        }
    }
}
