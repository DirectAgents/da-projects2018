using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class Affiliate
    {
        [NotMapped]
        public string DisplayName
        {
            get { return AffiliateName + " (" + AffiliateId + ")"; }
        }
    }
}
