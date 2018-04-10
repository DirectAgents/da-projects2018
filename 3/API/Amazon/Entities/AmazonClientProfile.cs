using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Entities
{
    public class Profile
    {
        public string ProfileId { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string Timezone { get; set; }
        public AccountInfo AccountInfo { get; set; }

    }
    public class AccountInfo
    {
        public string MarketplaceStringId { get; set; }
        public string BrandEntityId { get; set; }
        public string BrandName { get; set; }

    }
}
