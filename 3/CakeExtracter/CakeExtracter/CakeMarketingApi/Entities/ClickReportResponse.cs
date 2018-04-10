using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class ClickReportResponse
    {
        public int RowCount { get; set; }
        public bool Success { get; set; }
        public List<Click> Clicks { get; set; }
    }
}
