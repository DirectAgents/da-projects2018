using System.Windows.Forms.DataVisualization.Charting;

namespace DAGenerators.Charts
{
    public interface IChartBuilder
    {
        Chart Chart { get; }
        ChartArea MainChartArea { get; }
    }
}
