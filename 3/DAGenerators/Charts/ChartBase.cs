using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms.DataVisualization.Charting;

namespace DAGenerators.Charts
{
    public abstract class ChartBase
    {
        public IChartBuilder ChartBuilder { get; set; }
        public ChartImageFormat ChartImageFormat { get; set; }

        public MemoryStream MemoryStream { get; set; }

        protected void SetDefaults()
        {
            this.ChartImageFormat = ChartImageFormat.Png;

            this.ChartBuilder.Chart.Width = 800;
            this.ChartBuilder.MainChartArea.BackColor = Color.LightGray;
        }

        // NOTE: Be sure to call DisposeResources after calling this (i.e. after sending email)
        public LinkedResource GetAsLinkedResource(string contentId)
        {
            this.MemoryStream = new MemoryStream();

            this.ChartBuilder.Chart.SaveImage(this.MemoryStream, this.ChartImageFormat);
            this.MemoryStream.Seek(0, SeekOrigin.Begin);

            var linkedResource = new LinkedResource(this.MemoryStream);
            linkedResource.ContentId = contentId;

            return linkedResource;
        }

        public void DisposeResources()
        {
            if (this.MemoryStream != null)
                this.MemoryStream.Dispose();
        }
    }
}
