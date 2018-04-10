using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class CreativeMonthlySummary
    {
        public int CreativeId { get; set; }
        public DailySummary Summary { get; set; }
    }
}
