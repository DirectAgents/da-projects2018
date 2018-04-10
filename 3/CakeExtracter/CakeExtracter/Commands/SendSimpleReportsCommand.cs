using CakeExtracter.Common;
using CakeExtracter.Reports;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.Services;
using System.ComponentModel.Composition;
using System.Configuration;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SendSimpleReportsCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public SendSimpleReportsCommand()
        {
            IsCommand("sendSimpleReports", "send simple reports");
        }

        public override int Execute(string[] remainingArguments)
        {
            var gmailUsername = ConfigurationManager.AppSettings["GmailEmailer_Username"];
            var gmailPassword = ConfigurationManager.AppSettings["GmailEmailer_Password"];

            int numReportsSent;
            using (var db = new ClientPortalContext())
            {
                var reportManager = new SimpleReportManager(
                                            new ClientPortalRepository(db),
                                            new GmailEmailer(new System.Net.NetworkCredential(gmailUsername, gmailPassword))
                                        );
                numReportsSent = reportManager.CatchUp();
            }
            Logger.Info("Total reports sent: {0}", numReportsSent);
            return 0;
        }
    }
}
