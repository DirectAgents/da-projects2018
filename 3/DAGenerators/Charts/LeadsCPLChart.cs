using System.Collections;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DAGenerators.Charts
{
    public class LeadsCPLChart : ChartBase
    {
        public LeadsCPLChart(IEnumerable stats, bool sayTotal) // IEnumerable<SearchStat>
        {
            string prefix = sayTotal ? "Total " : "";
            string titleText = prefix + "Leads vs. CPL"; // make parameter/property?
            var builder = new TwoSeriesChartBuilder(titleText, prefix + "Leads", "CPL");

            builder.LeftSeries.ChartType = SeriesChartType.Area;
            builder.LeftSeries.Color = Color.FromArgb(240, 153, 203, 254);

            builder.RightSeries.ChartType = SeriesChartType.Line;
            builder.RightSeries.Color = Color.Red;
            builder.RightSeries.BorderWidth = 3;
            builder.RightSeries.MarkerStyle = MarkerStyle.Circle;
            builder.RightSeries.MarkerSize = 10;

            //builder.MainChartArea.BackColor = Color.White; // didn't work
            builder.MainChartArea.AxisY2.LabelStyle.Format = "C";

            builder.LeftSeries.Points.DataBind(stats, "Title", "TotalLeads", null);
            builder.RightSeries.Points.DataBind(stats, "Title", "CPL", null);

            this.ChartBuilder = builder;
            this.SetDefaults();
        }
    }
}
