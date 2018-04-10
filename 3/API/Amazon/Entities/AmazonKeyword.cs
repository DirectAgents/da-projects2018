using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Entities
{
    public class AmazonKeyword
    {
        public Int64 KeywordId { get; set; }
        public Int64 AdGroupId { get; set; }
        public Int64 CampaignId { get; set; }
        public string KeywordText { get; set; }
        public string MatchType { get; set; }
        public string State { get; set; }        
    }
}
