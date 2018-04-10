using ClientPortal.Data.Contexts;
using ClientPortal.Data.Contracts;
using ClientPortal.Data.DTOs;
using DAGenerators.Charts;
using DAGenerators.Spreadsheets;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;

namespace CakeExtracter.Reports
{
    class SearchReport : IReport
    {
        private readonly IClientPortalRepository cpRepo;
        private readonly SimpleReport simpleReport;

        public SearchReport(IClientPortalRepository cpRepo, SimpleReport simpleReport)
        {
            this.cpRepo = cpRepo;
            this.simpleReport = simpleReport;
        }

        public string Subject
        {
            get { return "Direct Agents Search Report"; }
        }

        public string Generate()
        {
            if (simpleReport.SearchProfile == null)
                throw new Exception("Cannot generate search report without a searchProfile");
            var searchProfile = simpleReport.SearchProfile;

            var fromDate = simpleReport.GetStatsStartDate();
            var toDate = simpleReport.GetStatsEndDate();

            var template = new SearchReportRuntimeTextTemplate
            {
                SearchProfile = searchProfile
            };
            template.Line1stat = this.cpRepo.GetSearchStats(searchProfile, fromDate, toDate, null);
            template.Line1stat.Title = string.Format("{0} - {1}", fromDate.ToShortDateString(), toDate.ToShortDateString());

            bool showLine2stat = false;
            if (simpleReport.PeriodMonths > 0)
            {
                fromDate = fromDate.AddMonths(-simpleReport.PeriodMonths);
                toDate = toDate.AddMonths(-simpleReport.PeriodMonths);
                showLine2stat = true;
            }
            else if (simpleReport.PeriodDays > 0)
            {
                fromDate = fromDate.AddDays(-simpleReport.PeriodDays);
                toDate = toDate.AddDays(-simpleReport.PeriodDays);
                showLine2stat = true;
            }
            if (showLine2stat)
            {
                template.Line2stat = this.cpRepo.GetSearchStats(searchProfile, fromDate, toDate, null);
                template.Line2stat.Title = string.Format("{0} - {1}", fromDate.ToShortDateString(), toDate.ToShortDateString());

                var firstStat = template.Line2stat;
                var secondStat = template.Line1stat;
                template.ChangeStat = new SimpleSearchStat
                {
                    Title = "Change",

                    // eCom
                    Revenue = secondStat.Revenue - firstStat.Revenue,
                    Cost = secondStat.Cost - firstStat.Cost,
                    ROAS = secondStat.ROAS - firstStat.ROAS,
                    Margin = secondStat.Margin - firstStat.Margin,
                    Orders = secondStat.Orders - firstStat.Orders,
                    CPO = secondStat.CPO - firstStat.CPO,

                    // leadgen
                    Clicks = secondStat.Clicks - firstStat.Clicks,
                    Impressions = secondStat.Impressions - firstStat.Impressions,
                    CTR = secondStat.CTR - firstStat.CTR,
                    // Spend (cost)
                    CPC = secondStat.CPC - firstStat.CPC,
                    // Leads (Orders)
                    CPL = secondStat.CPL - firstStat.CPL,
                    // ?ConvRate

                    Calls = secondStat.Calls - firstStat.Calls,
                    TotalLeads = secondStat.TotalLeads - firstStat.TotalLeads
                };
            }

            template.AcctMgrName = "";
            template.AcctMgrEmail = "";

            var searchProfileContacts = searchProfile.SearchProfileContactsOrdered;
            if (searchProfileContacts.Count() > 0)
            {
                var contact = searchProfileContacts.First().Contact;
                template.AcctMgrName = contact.FullName ?? "";
                template.AcctMgrEmail = contact.Email ?? "";
            }

            string content = template.TransformText();

            return content;
        }

        private ChartBase ChartObj { get; set; }
        private SpreadsheetBase Spreadsheet { get; set; }

        public AlternateView GenerateView()
        {
            var contentId = "chart1";
            var reportString = this.Generate();                                        //TODO: put in template?
            var htmlView = AlternateView.CreateAlternateViewFromString(reportString + "<table width=\"100%\"><tr><td style=\"text-align:center\"><img src=cid:" + contentId + "></td></tr></table>", null, "text/html");

            GenerateChart();
            var linkedResource = this.ChartObj.GetAsLinkedResource(contentId);
            htmlView.LinkedResources.Add(linkedResource);

            return htmlView;
        }

        private void GenerateChart()
        {
            var searchProfile = simpleReport.SearchProfile;
            int numWeeks = 8;
            var toDate = simpleReport.GetStatsEndDate();
            bool useAnalytics = false; // todo: get this from the SearchProfile when the column is moved there

            var stats = cpRepo.GetWeekStats(searchProfile, numWeeks, null, toDate).ToList();
            foreach (var stat in stats)
            {
                stat.Title = stat.Title.Replace(" ", "");
            }
            if (searchProfile.ShowRevenue)
                this.ChartObj = new OrdersCPOChart(stats);
            else
                this.ChartObj = new LeadsCPLChart(stats, searchProfile.ShowCalls);
        }

        public Attachment GenerateSpreadsheetAttachment()
        {
            Attachment attachment = null;
            GenerateSpreadsheet();
            if (this.Spreadsheet != null)
            {
                attachment = this.Spreadsheet.GetAsAttachment(simpleReport.Attachment_Filename);
            }
            return attachment;
        }
        private void GenerateSpreadsheet()
        {
            string templateFolder = ConfigurationManager.AppSettings["PATH_Search"];
            bool groupBySearchAccount = simpleReport.SearchProfile.HasChannelWithMultipleSearchAccounts();
            this.Spreadsheet = DAGenerators.Spreadsheets.GeneratorCP.GenerateSearchReport(
                cpRepo,
                templateFolder,
                simpleReport.SearchProfile.SearchProfileId,
                simpleReport.Attachment_NumWeeks,
                simpleReport.Attachment_NumMonths,
                simpleReport.GetStatsEndDate(),
                groupBySearchAccount
                );
        }

        public void DisposeResources()
        {
            if (this.ChartObj != null)
                this.ChartObj.DisposeResources();
            if (this.Spreadsheet != null)
                this.Spreadsheet.DisposeResources();
        }
    }
}
