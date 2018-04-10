using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class SearchAccount
    {
        public const string GoogleChannel = "Google";
        public const string BingChannel = "Bing";
        public const string AppleChannel = "Apple";
        public const string CriteoChannel = "Criteo";

        [NotMapped]
        public bool UseConvertedClicks { get; set; }
        [NotMapped]
        public DateTime? EarliestStat { get; set; }
        [NotMapped]
        public DateTime? LatestStat { get; set; }
    }
}
