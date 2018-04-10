using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CakeExtracter.Common;
using ClientPortal.Data.Contexts;
using Criteo;
using Criteo.CriteoAPI;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class CriteoApiExtracter2 : Extracter<SearchDailySummary>
    {
        private readonly string accountCode;
        private readonly DateTime beginDate;
        private readonly DateTime endDate;
        private readonly int timezoneOffset;

        private CriteoUtility _criteoUtility;

        public CriteoApiExtracter2(string accountCode, DateRange dateRange, int timezoneOffset)
        {
            this.accountCode = accountCode;
            this.beginDate = dateRange.FromDate;
            this.endDate = dateRange.ToDate;
            this.timezoneOffset = timezoneOffset;
            _criteoUtility = new CriteoUtility(m => Logger.Info(m), m => Logger.Warn(m));
            _criteoUtility.SetCredentials(accountCode);
        }

        public campaign[] GetCampaigns()
        {
            return _criteoUtility.GetCampaigns();
        }

        protected override void Extract()
        {
            Logger.Info("Extracting SearchDailySummaries from Criteo API for {0} from {1:d} to {2:d}",
                        this.accountCode, this.beginDate, this.endDate);

            var adjustedBeginDate = this.beginDate;
            var adjustedEndDate = this.endDate;
            if (this.timezoneOffset < 0)
                adjustedEndDate = adjustedEndDate.AddDays(1);
            else if (this.timezoneOffset > 0)
                adjustedBeginDate = adjustedBeginDate.AddDays(-1);

            var reportUrl = _criteoUtility.GetCampaignReport(adjustedBeginDate, adjustedEndDate, true);

            var dailySummaries = EnumerateDailySummaries(reportUrl);
            Add(dailySummaries);
            End();
        }

        private IEnumerable<SearchDailySummary> EnumerateDailySummaries(string reportUrl)
        {
            IEnumerable<SearchDailySummary> hourlySummaries = EnumerateHourlyXmlReportRows(reportUrl).ToList();
            hourlySummaries = hourlySummaries.Where(s => s.Date >= this.beginDate && s.Date <= this.endDate);
            var dailyGroups = hourlySummaries.GroupBy(s => new { s.SearchCampaignId, s.Date });
            foreach (var group in dailyGroups)
            {
                var sds = new SearchDailySummary
                {
                    SearchCampaignId = group.Key.SearchCampaignId,
                    Date = group.Key.Date,
                    Revenue = group.Sum(g => g.Revenue),
                    Cost = group.Sum(g => g.Cost),
                    Orders = group.Sum(g => g.Orders),
                    Clicks = group.Sum(g => g.Clicks),
                    Impressions = group.Sum(g => g.Impressions)
                };
                yield return sds;
            }
        }

        private IEnumerable<SearchDailySummary> EnumerateHourlyXmlReportRows(string reportUrl)
        {
            using (var reader = XmlReader.Create(reportUrl))
            {
                var columnNames = new List<string>(new[] { "campaignID", "dateTime" });
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "column":
                                    if (reader.MoveToAttribute("name"))
                                    {
                                        columnNames.Add(reader.Value);
                                    }
                                    break;
                                case "row":
                                    {
                                        var row = new Dictionary<string, string>();
                                        foreach (var columnName in columnNames)
                                        {
                                            if (reader.MoveToAttribute(columnName))
                                            {
                                                row.Add(reader.Name, reader.Value);
                                            }
                                            else
                                                throw new Exception("could not move to column " + columnName);
                                        }
                                        var sds = new SearchDailySummary
                                        {
                                            SearchCampaignId = int.Parse(row["campaignID"]),
                                            Date = DateTime.Parse(row["dateTime"]).AddHours(timezoneOffset).Date,
                                            Revenue = decimal.Parse(row["orderValue"]),
                                            Cost = decimal.Parse(row["cost"]),
                                            Orders = int.Parse(row["sales"]),
                                            Clicks = int.Parse(row["click"]),
                                            Impressions = int.Parse(row["impressions"])
                                        };
                                        yield return sds;
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }
        }

    }
}
