using System;
namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class DailySummary
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int Clicks { get; set; }
        public decimal ClickThru { get; set; }
        public int Conversions { get; set; }
        public int Paid { get; set; }
        public int Sellable { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal CPL { get; set; }
        public decimal Cost { get; set; }
        public decimal RPT { get; set; }
        public decimal Revenue { get; set; }
        public decimal Margin { get; set; }
        public decimal Profit { get; set; }
        public decimal EPC { get; set; }
    }
}
