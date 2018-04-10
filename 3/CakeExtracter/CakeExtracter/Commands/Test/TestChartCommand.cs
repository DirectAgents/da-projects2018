using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms.DataVisualization.Charting;
using CakeExtracter.Common;
using CakeExtracter.Reports;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.DTOs;
using ClientPortal.Data.Services;
using DAGenerators.Charts;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestChartCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestChartCommand()
        {
            IsCommand("testChart");
        }

        public override int Execute(string[] remainingArguments)
        {
            //var chart = CreateAChart();
            var chart = CreateAChartWithBuilder();
            //var chart = CreateAChartWithChartBase();

            SaveToDisk(chart);
            //SendViaEmail(chart);

            return 0;
        }

        private Chart CreateAChart()
        {
            var chart = new Chart();
            //chart.Width = 400;
            //chart.Height = 400;
            //chart.BackColor = Color.LightSalmon;

            var series = new Series("bla");
            series.ChartType = SeriesChartType.FastLine;

            for (int x = 0; x <= 5; x++)
                series.Points.AddXY("hi" + x, (x * 5));
            chart.Series.Add(series);

            ChartArea ca = new ChartArea("ca1");
            ca.BackColor = Color.Gray;
            chart.ChartAreas.Add(ca);

            return chart;
        }

        private Chart CreateAChartWithChartBase()
        {
            var stats = GetStats();
            var ordersCPOChart = new OrdersCPOChart(stats);
            return ordersCPOChart.ChartBuilder.Chart;
        }

        private Chart CreateAChartWithBuilder()
        {
            var builder = new TwoSeriesChartBuilder("Orders vs. CPO", "Orders", "CPO");
            builder.Chart.Width = 880;

            builder.LeftSeries.ChartType = SeriesChartType.Area;
            builder.LeftSeries.Color = Color.FromArgb(175, 153, 203, 254);

            builder.RightSeries.ChartType = SeriesChartType.Line;
            builder.RightSeries.Color = Color.Red;
            builder.RightSeries.BorderWidth = 3;
            builder.RightSeries.MarkerStyle = MarkerStyle.Circle;
            builder.RightSeries.MarkerSize = 10;

            //builder.MainChartArea.BackColor = Color.Transparent;
            builder.MainChartArea.AxisY2.LabelStyle.Format = "C";

            var stats = GetStats();
            builder.LeftSeries.Points.DataBind(stats, "Title", "Orders", null);
            builder.RightSeries.Points.DataBind(stats, "Title", "CPO", null);

            return builder.Chart;
        }

        private IEnumerable<SearchStat> GetStats()
        {
            int profileId = 6; //schol printables
            //int profileId = 7; //schol teacher express
            int numWeeks = 5;
            bool useAnalytics = false;

            IEnumerable<SearchStat> stats;
            using (var db = new ClientPortalContext())
            {
                var cpRepo = new ClientPortalRepository(db);
                var searchProfile = cpRepo.GetSearchProfile(profileId);
                stats = cpRepo.GetWeekStats(searchProfile, numWeeks, null, null);
            }
            return stats;
        }

        private void SaveToDisk(Chart chart)
        {
            chart.SaveImage("c:\\users\\kslesinsky\\downloads\\test123.png", ChartImageFormat.Png);
        }

        private void SendViaEmail(Chart chart)
        {
            var sendTo = "kevin@directagents.com";

            var gmailUsername = ConfigurationManager.AppSettings["GmailEmailer_Username"];
            var gmailPassword = ConfigurationManager.AppSettings["GmailEmailer_Password"];
            var emailer = new GmailEmailer(new NetworkCredential(gmailUsername, gmailPassword));

            var plainView = AlternateView.CreateAlternateViewFromString("this is the plain view", null, "text/plain");
            var htmlView = AlternateView.CreateAlternateViewFromString("here's an embedded image:<br/><img src=cid:test123>", null, "text/html");

            //var resource = new LinkedResource("c:\\users\\kslesinsky\\downloads\\test123.png");
            using (var ms = new MemoryStream())
            {
                chart.SaveImage(ms, ChartImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                var resource = new LinkedResource(ms);
                resource.ContentId = "test123";
                htmlView.LinkedResources.Add(resource);

                emailer.SendEmail("ignored@directagents.com", new[] { sendTo }, null, "test image", new[] { plainView, htmlView });
            }
        }
    }
}
