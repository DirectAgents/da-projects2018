
namespace EomTool.Domain.Entities
{
    public class PublisherSummary
    {
        public int affid { get; set; }
        public string PublisherName { get; set; }
        public string Currency { get; set; }
        public decimal PayoutTotal { get; set; }
        public decimal MinPctMargin { get; set; }
        public decimal MaxPctMargin { get; set; }
        public string BatchIds { get; set; }
        public string LatestNote { get; set; }
        public string NetTerms { get; set; }
    }
}
