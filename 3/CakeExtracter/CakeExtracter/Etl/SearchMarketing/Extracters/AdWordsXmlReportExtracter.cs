using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class AdWordsXmlReportExtracter : Extracter<Dictionary<string, string>>
    {
        private readonly string xmlFilePath;
        private readonly string accountName;

        public AdWordsXmlReportExtracter(string xmlFilePath, string accountName)
        {
            this.xmlFilePath = xmlFilePath;
            this.accountName = accountName;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting SearchDailySummaries for {0} from {1}", accountName, xmlFilePath);
            var items = EnumerateRows(); //.Where(c => c["account"] == accountName);
            Add(items);
            End();
        }

        private IEnumerable<Dictionary<string, string>> EnumerateRows()
        {
            using (var reader = XmlReader.Create(xmlFilePath))
            {
                var columnNames = new List<string>();
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

                                        // FOR TESTING !!!
                                        row.Add("account", accountName);

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
