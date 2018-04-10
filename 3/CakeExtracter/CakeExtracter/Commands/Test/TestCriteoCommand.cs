using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml;
using CakeExtracter.Common;
using Criteo;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestCriteoCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestCriteoCommand()
        {
            IsCommand("testCriteo");
        }

        public override int Execute(string[] remainingArguments)
        {
            var criteoUtility = new CriteoUtility(m => Logger.Info(m), m => Logger.Warn(m));
            //var campaigns = criteoUtility.GetCampaigns(25296);
            var reportUrl = criteoUtility.GetCampaignReport(new DateTime(2014, 10, 8), new DateTime(2014, 10, 8));
            GetReport(reportUrl);
            return 0;
        }

        private void GetReport(string reportUrl)
        {
            var rows = EnumerateCriteoXmlReportRows(reportUrl);
            foreach (var row in rows)
            {
                Logger.Info("row count: {0}", row.Count);
            }
        }

        private static IEnumerable<Dictionary<string, string>> EnumerateCriteoXmlReportRows(string reportUrl)
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
