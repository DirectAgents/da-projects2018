using System.ComponentModel.Composition;
using CakeExtracter.Common;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using CakeExtracter.Etl.SearchMarketing.Loaders;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchSearchDailySummariesXmlCommand : ConsoleCommand
    {
        public string XmlFile { get; set; }
        public string AccountName { get; set; }

        public override void ResetProperties()
        {
            XmlFile = null;
            AccountName = null;
        }

        public SynchSearchDailySummariesXmlCommand()
        {
            IsCommand("synchSearchDailySummariesXml",
                      "synch SearchDailySummaries from Google AdWords Xml Report");
            HasRequiredOption("f|xmlFile=", "XML File", c => XmlFile = c);
            HasRequiredOption("a|account=", "Account Name", c => AccountName = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new AdWordsXmlReportExtracter(XmlFile, AccountName);
            var loader = new AdWordsLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
            return 0;
        }

    }
}
