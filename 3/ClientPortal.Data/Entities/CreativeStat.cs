using ClientPortal.Data.Contexts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public class CreativeStat_Validation
    {
        [Display(Name = "Conversions")]
        public int? Leads { get; set; }
    }

    [MetadataType(typeof(CreativeStat_Validation))]
    public partial class CreativeStat
    {
        [NotMapped]
        public decimal? ClickThroughRate
        {
            get
            {
                if (Clicks == null || CampaignDrop.Opens == null || CampaignDrop.Opens.Value == 0)
                    return null;
                else
                    return (decimal)Clicks.Value / CampaignDrop.Opens.Value;
            }
        }

        [NotMapped]
        public decimal? ConversionRate
        {
            get
            {
                if (Leads == null || Clicks == null || Clicks.Value == 0)
                    return null;
                else
                    return (decimal)Leads.Value / Clicks.Value;
            }
        }

        public void SetPropertiesFrom(CreativeStat inStat)
        {
            this.CreativeId = inStat.CreativeId;
            this.Clicks = inStat.Clicks;
            this.Leads = inStat.Leads;
        }

        //not used??
        public bool AnyChanges(CreativeStat inStat)
        {
            bool anyChanges = false;

            if (this.CreativeId != inStat.CreativeId ||
                this.Clicks != inStat.Clicks ||
                this.Leads != inStat.Leads)
                anyChanges = true;

            return anyChanges;
        }
    }
}
