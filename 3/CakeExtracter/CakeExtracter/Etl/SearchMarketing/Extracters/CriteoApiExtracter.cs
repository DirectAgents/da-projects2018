using CakeExtracter.Common;
using Criteo;
using Criteo.CriteoAPI;
using System;
using System.Collections.Generic;
using System.Xml;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class CriteoApiExtracter : Extracter<Dictionary<string, string>>
    {
        private readonly string accountCode;
        private readonly DateTime beginDate;
        private readonly DateTime endDate;

        private CriteoUtility _criteoUtility;

        public CriteoApiExtracter(string accountCode, DateRange dateRange)
        {
            this.accountCode = accountCode;
            this.beginDate = dateRange.FromDate;
            this.endDate = dateRange.ToDate;
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

            var reportUrl = _criteoUtility.GetCampaignReport(beginDate, endDate);

            var reportRows = EnumerateCriteoXmlReportRows(reportUrl);
            Add(reportRows);
            End();
        }

        public static IEnumerable<Dictionary<string, string>> EnumerateCriteoXmlReportRows(string reportUrl)
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
                                        yield return row;
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
