
namespace DAGenerators.Spreadsheets
{
    public class SearchReportLeadGenBase : SearchReportPPC
    {
        protected void SetupSheet2()
        {
            Metrics2.Title = new Metric(2, null) { MemberName = "Title" };
            Metrics2.Clicks1 = new Metric(3, "ClicksYr1") { MemberName = "PrevClicks" };
            Metrics2.Clicks2 = new Metric(4, "ClicksYr2") { MemberName = "Clicks_" };
            Metrics2.Impressions1 = new Metric(5, "ImpressionsYr1") { MemberName = "PrevImpressions" };
            Metrics2.Impressions2 = new Metric(6, "ImpressionsYr2") { MemberName = "Impressions_" };
            Metrics2.CTR1 = new Metric(7, "CTR_Yr1");
            Metrics2.CTR2 = new Metric(8, "CTR_Yr2");
            Metrics2.Cost1 = new Metric(9, "SpendYr1") { MemberName = "PrevCost" };
            Metrics2.Cost2 = new Metric(10, "SpendYr2") { MemberName = "Cost_" };
            Metrics2.CPC1 = new Metric(11, "CPC_Yr1");
            Metrics2.CPC2 = new Metric(12, "CPC_Yr2");
            Metrics2.TotalLeads1 = new Metric(13, "LeadsYr1") { MemberName = "PrevTotalLeads" };
            Metrics2.TotalLeads2 = new Metric(14, "LeadsYr2") { MemberName = "TotalLeads_" };
            Metrics2.CPL1 = new Metric(15, "CPL_Yr1");
            Metrics2.CPL2 = new Metric(16, "CPL_Yr2");
        }

        public override void CreateYearOverYear_Charts()
        {
            CreateChart2_YOY(Metrics2.Title.ColNum, Metrics2.CTR1, Metrics2.CTR2, Metrics2.CPC1, Metrics2.CPC2, false, "YoY CTR vs. CPC");
            CreateChart2_YOY(Metrics2.Title.ColNum, Metrics2.TotalLeads1, Metrics2.TotalLeads2, Metrics2.CPL1, Metrics2.CPL2, true, "YoY Leads vs. CPL");
        }
    }

    public class SearchReportLeadGen : SearchReportLeadGenBase
    {
        protected override void Setup()
        {
            TemplateFilename = "SearchTemplateLeadGen.xlsx";

            Metrics1.Title = new Metric(2, null) { MemberName = "Title" };
            Metrics1.Clicks = new Metric(3, "Clicks") { MemberName = "Clicks_" };
            Metrics1.Impressions = new Metric(4, "Impressions") { MemberName = "Impressions_" };
            Metrics1.CTR = new Metric(5, "CTR");
            Metrics1.Cost = new Metric(6, "Spend") { MemberName = "Cost_" };
            Metrics1.CPC = new Metric(7, "CPC");
            Metrics1.Orders = new Metric(8, "Leads") { MemberName = "Orders_" };
            Metrics1.CPL = new Metric(9, "CPL");
            Metrics1.OrderRate = new Metric(10, "Conv Rate");

            SetupSheet2();
        }

        public override void CreateCharts(bool weeklyNotMonthly)
        {
            CreateChart(Metrics1.Title.ColNum, Metrics1.CTR, Metrics1.CPC, false, weeklyNotMonthly);
            CreateChart(Metrics1.Title.ColNum, Metrics1.Orders, Metrics1.CPL, true, weeklyNotMonthly);
        }
    }

    public class SearchReportLeadGenWithCalls : SearchReportLeadGenBase
    {
        protected override void Setup()
        {
            TemplateFilename = "SearchTemplateLeadGenWithCalls.xlsx";

            Metrics1.Title = new Metric(2, null) { MemberName = "Title" };
            Metrics1.Clicks = new Metric(3, "Clicks") { MemberName = "Clicks_" };
            Metrics1.Impressions = new Metric(4, "Impressions") { MemberName = "Impressions_" };
            Metrics1.CTR = new Metric(5, "CTR");
            Metrics1.Cost = new Metric(6, "Spend") { MemberName = "Cost_" };
            Metrics1.CPC = new Metric(7, "CPC");
            Metrics1.Orders = new Metric(8, "Leads") { MemberName = "Orders_" };
            Metrics1.Calls = new Metric(9, "Calls") { MemberName = "Calls_" };
            Metrics1.TotalLeads = new Metric(10, "Total Leads_");
            Metrics1.CPL = new Metric(11, "CPL");
            Metrics1.OrderRate = new Metric(12, "Conv Rate");

            SetupSheet2();
        }

        public override void CreateCharts(bool weeklyNotMonthly)
        {
            CreateChart(Metrics1.Title.ColNum, Metrics1.CTR, Metrics1.CPC, false, weeklyNotMonthly);
            CreateChart(Metrics1.Title.ColNum, Metrics1.TotalLeads, Metrics1.CPL, true, weeklyNotMonthly);
        }
    }
}
