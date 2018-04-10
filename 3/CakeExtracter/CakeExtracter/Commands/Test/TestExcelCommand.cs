using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using CakeExtracter.Common;
using CakeExtracter.Reports;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.DTOs;
using ClientPortal.Data.Services;
using DAGenerators.Spreadsheets;
using OfficeOpenXml;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestExcelCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestExcelCommand()
        {
            IsCommand("testExcel");
        }

        public override int Execute(string[] remainingArguments)
        {
            //Test();
            //TestWithSpreadsheets();
            //TestWithLoad();
            TestWithTemplate();
            return 0;
        }

        private void Test()
        {
            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Title = "Whatever";

                p.Workbook.Worksheets.Add("sample worksheet");
                var ws = p.Workbook.Worksheets[1];
                ws.Cells[1, 1].Value = "hello";

                //SaveToFile(p);
                SendViaEmail(p);
            }
        }
        private void TestWithSpreadsheets()
        {
            var spreadsheet = new TestSpreadSheet();

            SaveToFile(spreadsheet.ExcelPackage);
            //var attachment = spreadsheet.GetAsAttachment("report123.xlsx");
            //SendViaEmail(attachment);
            spreadsheet.DisposeResources();
        }
        private void TestWithLoad()
        {
            string filename = ConfigurationManager.AppSettings["ExcelTemplate_SearchPPC"]; // SearchPPCtemplate.xlsx
            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename));
            using (var p = new ExcelPackage(file))
            {
                var worksheet = p.Workbook.Worksheets[1];
            }
        }
        private void TestWithTemplate()
        {
            string templateFolder = ConfigurationManager.AppSettings["PATH_Search"];

            var spreadsheet = new SearchReportPPC();
            spreadsheet.Setup(templateFolder);
            var stats = GetStats(false);
            spreadsheet.LoadMonthlyStats(stats);
            stats = GetStats(true);
            spreadsheet.LoadWeeklyStats(stats);
            //spreadsheet.SetClientName("Scholastic, Inc.");

            SaveToFile(spreadsheet.ExcelPackage);
            //var attachment = spreadsheet.GetAsAttachment("reportABC.xlsx");
            //SendViaEmail(attachment);
            spreadsheet.DisposeResources();
        }

        private IEnumerable<SearchStat> GetStats(bool weeklyNotMonthly)
        {
            //int profileId = 6; //schol printables
            //int profileId = 7; //schol teacher express
            int profileId = 11; //schol class mags

            IEnumerable<SearchStat> stats;
            using (var cpRepo = new ClientPortalRepository(new ClientPortalContext()))
            {
                var searchProfile = cpRepo.GetSearchProfile(profileId);
                if (weeklyNotMonthly)
                {
                    int numWeeks = 6;
                    stats = cpRepo.GetWeekStats(searchProfile, numWeeks, null, null);
                }
                else
                {
                    int numMonths = 6;
                    var endDate = DateTime.Today.AddDays(-1);
                    bool yoy = true;
                    stats = cpRepo.GetMonthStats(searchProfile, numMonths, null, endDate, yoy);
                }
            }
            return stats;
        }

        private void SaveToFile(ExcelPackage p)
        {
            Byte[] bin = p.GetAsByteArray();
            string path = "c:\\users\\kslesinsky\\downloads\\test123.xlsx";
            File.WriteAllBytes(path, bin);
        }

        private void SendViaEmail(ExcelPackage p)
        {
            using (var ms = new MemoryStream())
            {
                p.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var attachment = new Attachment(ms, "report.xlsx", "application/vnd.ms-excel");
                SendViaEmail(attachment);
            }
        }

        private void SendViaEmail(Attachment attachment)
        {
            var sendTo = "kevin@directagents.com";
            var gmailUsername = ConfigurationManager.AppSettings["GmailEmailer_Username"];
            var gmailPassword = ConfigurationManager.AppSettings["GmailEmailer_Password"];
            var emailer = new GmailEmailer(new NetworkCredential(gmailUsername, gmailPassword));

            var plainView = AlternateView.CreateAlternateViewFromString("this is the plain view", null, "text/plain");
            var htmlView = AlternateView.CreateAlternateViewFromString("this is the <b>html</b> view", null, "text/html");

            emailer.SendEmail("ignored@directagents.com", new[] { sendTo }, null, "test excel attachment", new[] { plainView, htmlView }, attachment);
        }
    }
}
