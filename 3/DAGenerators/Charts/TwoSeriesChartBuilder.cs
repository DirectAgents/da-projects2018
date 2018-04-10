using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DAGenerators.Charts
{
    public class TwoSeriesChartBuilder : IChartBuilder
    {
        // Implement from IChartBuilder
        public Chart Chart { get; set; }
        public ChartArea MainChartArea
        {
            get { return Chart.ChartAreas[0]; }
        }

        // Other properties
        public Series LeftSeries
        {
            get { return Chart.Series[0]; }
        }
        public Series RightSeries
        {
            get { return Chart.Series[1]; }
        }

        public TwoSeriesChartBuilder(string titleText, string leftSeriesName, string rightSeriesName)
        {
            Chart = new Chart();
            var title = Chart.Titles.Add("title1");
            title.Text = titleText;

            Chart.Series.Add(new Series(leftSeriesName));
            Chart.Series.Add(new Series(rightSeriesName));
            Chart.Series[1].YAxisType = AxisType.Secondary;

            var ca = Chart.ChartAreas.Add("area1");
            //ca.Area3DStyle.Enable3D = true;
            //ca.Area3DStyle.Inclination = 5;
            //ca.Area3DStyle.Rotation = 5;
            ca.AxisX.Interval = 1;
            ca.AxisX.LabelStyle.Angle = 0;
            ca.AxisY.Title = leftSeriesName;
            ca.AxisY2.Title = rightSeriesName;
            ca.AxisY2.MajorGrid.Enabled = false;

            var legend = Chart.Legends.Add("legend1");
            legend.DockedToChartArea = "area1";
            legend.Docking = Docking.Left;
        }
        // To set after constructing:
        // Chart.Width / Chart.Height
        // Left(or Right)Series.ChartType / BorderWidth(for SeriesChartType.Line)
        // MainChartArea.BackColor
        // MainChartArea.AxisY(or AxisY2).LabelStyle.Format
        // Left(or Right)Series.Points.DataBind(stats, "Title", "Orders", null)
    }

}
