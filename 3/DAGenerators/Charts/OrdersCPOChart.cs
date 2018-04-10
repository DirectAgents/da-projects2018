using System.Collections;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DAGenerators.Charts
{
    public class OrdersCPOChart : ChartBase
    {
        public OrdersCPOChart(IEnumerable stats) // IEnumerable<SearchStat>
        {
            string titleText = "Orders vs. CPO"; // make parameter/property?
            var builder = new TwoSeriesChartBuilder(titleText, "Orders", "CPO");

            builder.LeftSeries.ChartType = SeriesChartType.Area;
            builder.LeftSeries.Color = Color.FromArgb(240, 153, 203, 254);

            builder.RightSeries.ChartType = SeriesChartType.Line;
            builder.RightSeries.Color = Color.Red;
            builder.RightSeries.BorderWidth = 3;
            builder.RightSeries.MarkerStyle = MarkerStyle.Circle;
            builder.RightSeries.MarkerSize = 10;

            //builder.MainChartArea.BackColor = Color.White; // didn't work
            builder.MainChartArea.AxisY2.LabelStyle.Format = "C";

            builder.LeftSeries.Points.DataBind(stats, "Title", "Orders", null);
            builder.RightSeries.Points.DataBind(stats, "Title", "CPO", null);

            this.ChartBuilder = builder;
            this.SetDefaults();
        }
    }
}
