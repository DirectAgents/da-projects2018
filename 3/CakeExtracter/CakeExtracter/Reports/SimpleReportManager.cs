using ClientPortal.Data.Contexts;
using ClientPortal.Data.Contracts;
using DAGenerators.Charts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Windows.Forms.DataVisualization.Charting;

namespace CakeExtracter.Reports
{
    public class SimpleReportManager
    {
        private const DayOfWeek DefaultStartDayOfWeek = DayOfWeek.Monday; //TODO: make app setting

        private readonly IClientPortalRepository cpRepo;
        private readonly GmailEmailer emailer;

        public SimpleReportManager(IClientPortalRepository cpRepo, GmailEmailer emailer)
        {
            this.cpRepo = cpRepo;
            this.emailer = emailer;
        }

        // This can be called to send test reports (without updating Last and NextSend)
        public int SendReports(SimpleReport simpleReport, string overrideEmail = null) //allow to override nextsend, perioddays, periodmonths...
        {
            if (String.IsNullOrWhiteSpace(overrideEmail) && String.IsNullOrWhiteSpace(simpleReport.Email))
                return 0;

            int reportsSent = 0;
            var iReports = CreateIReports(simpleReport);
            foreach (var iReport in iReports)
            {
                if (SendReport(simpleReport, iReport, overrideEmail))
                    reportsSent++;
            }
            return reportsSent;
        }

        // returns # of reports sent
        public int CatchUp()
        {
            int totalReportsSent = 0;
            var reportsToSend = GetReportsToSend();
            while (reportsToSend.Count() > 0)
            {
                Logger.Info("SimpleReports this round: {0}", reportsToSend.Count());
                foreach (var simpleReport in reportsToSend)
                {
                    CheckInitialize(simpleReport);

                    // For now... until these are added to the SimpleReport database table:
                    simpleReport.IncludeAttachment = true;
                    simpleReport.Attachment_NumMonths = SimpleReport.DefaultNumMonths;
                    simpleReport.Attachment_NumWeeks = SimpleReport.DefaultNumWeeks;
                    simpleReport.Attachment_Filename = simpleReport.GetDefaultFilename();

                    int reportsSent = SendReports(simpleReport);
                    totalReportsSent += reportsSent;

                    Logger.Info("Sent {0} report(s) for {1}", reportsSent, simpleReport.ParentName);

                    if (reportsSent > 0)
                        UpdateLastAndNextSend(simpleReport);
                    else
                    {
                        Logger.Warn("Couldn't send reports. Disabling.");
                        simpleReport.Enabled = false;
                        cpRepo.SaveChanges();
                    }
                }
                // see if any more reports need to be sent
                reportsToSend = GetReportsToSend();
            }
            Cleanup();
            return totalReportsSent;
        }

        private IEnumerable<SimpleReport> GetReportsToSend()
        {
            var now = DateTime.Now;
            var reportsToSend = cpRepo.SimpleReports.Where(r => r.Enabled && (r.NextSend == null || now >= r.NextSend));
            return reportsToSend.ToList();
        }

        // If null, set NextSend to the day after the end of the last stats period.
        // Also initializes PeriodDays if PeriodMonths and PeriodDays are both 0.
        private void CheckInitialize(SimpleReport simpleReport)
        {
            if (simpleReport.NextSend == null)
            {
                var today = DateTime.Today;
                if (simpleReport.PeriodMonths > 0)
                {
                    // Set to the 1st of this month, so last month's stats will be sent
                    simpleReport.NextSend = new DateTime(today.Year, today.Month, 1);
                }
                else
                {
                    if (simpleReport.PeriodDays <= 0)
                    {
                        simpleReport.PeriodDays = SimpleReport.DefaultPeriodDays;
                        simpleReport.PeriodMonths = SimpleReport.DefaultPeriodMonths;
                    }

                    simpleReport.NextSend = today;

                    DayOfWeek? startDayOfWeek = simpleReport.GetStartDayOfWeek();
                    if (simpleReport.PeriodDays == 7 && startDayOfWeek.HasValue)
                    {   // Set to the StartDayOfWeek that's today or most recently passed
                        while (simpleReport.NextSend.Value.DayOfWeek != startDayOfWeek.Value)
                        {
                            simpleReport.NextSend = simpleReport.NextSend.Value.AddDays(-1);
                        }
                    }
                }
                cpRepo.SaveChanges();
            }
        }

        // the IReports are the objects used to generate the report text
        private IEnumerable<IReport> CreateIReports(SimpleReport simpleReport)
        {
            List<IReport> iReports = new List<IReport>();

            if (simpleReport.Advertiser != null)
            {
                iReports.Add(new CakeReport(cpRepo, simpleReport.Advertiser, simpleReport.NextSend.Value.AddDays(-1), simpleReport.PeriodDays));
                                 //TODO: Handle the case where PeriodDays==0 and PeriodMonths>0
            }
            if (simpleReport.SearchProfile != null)
            {
                iReports.Add(new SearchReport(cpRepo, simpleReport));
            }
            return iReports;
        }

        private bool SendReport(SimpleReport simpleReport, IReport iReport, string overrideEmail = null)
        {
            try
            {
                emailer.GenerateAndSendSimpleReport(simpleReport, iReport, overrideEmail);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
            finally
            {
                iReport.DisposeResources();
            }
        }

        private void UpdateLastAndNextSend(SimpleReport rep)
        {
            var today = DateTime.Today;
            rep.LastSend = DateTime.Now;

            rep.LastStatsDate = (rep.NextSend ?? today).AddDays(-1);

            rep.NextSend = rep.NextSend ?? today; // to ensure it's not null
            if (rep.PeriodMonths > 0)
            {
                rep.NextSend = rep.NextSend.Value.AddMonths(rep.PeriodMonths);
            }
            else if (rep.PeriodDays > 0)
            {
                rep.NextSend = rep.NextSend.Value.AddDays(rep.PeriodDays);
            }
            else // both PeriodMonths and PeriodDays are unset for the report; use default period
            {
                rep.NextSend = rep.NextSend.Value.AddMonths(SimpleReport.DefaultPeriodMonths);
                rep.NextSend = rep.NextSend.Value.AddDays(SimpleReport.DefaultPeriodDays);
            }
            cpRepo.SaveChanges();
        }

        private void Cleanup()
        {
            //var today = DateTime.Today;
            //var disabledReportsInThePast = cpRepo.SimpleReports.Where(sr => !sr.Enabled && sr.NextSend < today);
            //foreach (var rep in disabledReportsInThePast)
            //{
            //    rep.NextSend = null;
            //    // so that multiple reports won't be sent unexpectedly once re-enabled
            //}
            //cpRepo.SaveChanges();
        }
    }
}
