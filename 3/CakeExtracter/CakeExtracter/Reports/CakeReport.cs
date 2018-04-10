using ClientPortal.Data.Contexts;
using ClientPortal.Data.Contracts;
using System;
using System.Net.Mail;

namespace CakeExtracter.Reports
{
    class CakeReport : IReport
    {
        private readonly IClientPortalRepository cpRepo;
        private readonly Advertiser advertiser;
        private readonly DateTime EndDate;
        private readonly int PeriodDays;

        public CakeReport(IClientPortalRepository cpRepo, Advertiser advertiser, DateTime endDate, int periodDays)
        {
            this.cpRepo = cpRepo;
            this.advertiser = advertiser;
            this.EndDate = endDate;
            this.PeriodDays = periodDays;
        }

        public string Subject
        {
            get { return "Direct Agents Client Portal Automated Report"; }
        }

        public string Generate()
        {
            var startDate = EndDate.AddDays(-1 * PeriodDays + 1);
            var dateRangeSummary = this.cpRepo.GetDateRangeSummary(startDate, EndDate, advertiser.AdvertiserId, null, advertiser.ShowConversionData);
            if (dateRangeSummary == null)
            {
                var msg = String.Format("Cannot generate report for advertiser id {0}, stats not synched?", advertiser.AdvertiserId);
                throw new Exception(msg);
            }

            var template = new CakeReportRuntimeTextTemplate();
            template.AdvertiserName = advertiser.AdvertiserName ?? "";
            template.Week = string.Format("{0} - {1}", startDate.ToShortDateString(), EndDate.ToShortDateString());
            template.Clicks = dateRangeSummary.Clicks;
            template.Leads = dateRangeSummary.Conversions;
            template.Rate = dateRangeSummary.ConversionRate;
            template.Spend = dateRangeSummary.Revenue;

            template.ConversionValueName = "";
            template.Conv = "";
            template.AcctMgrName = "";
            template.AcctMgrEmail = "";

            if (advertiser.ShowConversionData)
            {
                template.ConversionValueName = advertiser.ConversionValueName ?? "";
                template.Conv = (dateRangeSummary.ConVal != null) ? dateRangeSummary.ConVal.Value.ToString("#,0.##") : "";
            }
            var acctManager = advertiser.AccountManagerContact;
            if (acctManager != null)
            {
                template.AcctMgrName = acctManager.FullName;
                template.AcctMgrEmail = acctManager.Email;
            }
            else
            {
                template.AcctMgrName = "";
                template.AcctMgrEmail = "";
            }

            string content = template.TransformText();

            return content;
        }

        public AlternateView GenerateView()
        {
            var reportString = this.Generate();
            var htmlView = AlternateView.CreateAlternateViewFromString(reportString, null, "text/html");
            return htmlView;
        }

        public Attachment GenerateSpreadsheetAttachment()
        {
            return null;
        }

        public void DisposeResources()
        {
        }
    }
}
