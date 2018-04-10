using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DirectAgents.Domain.Entities.Wiki
{
    public class Campaign_Metadata
    {
        public int Pid { get; set; }

        public string DefaultPriceFormat { get; set; } // e.g. CPA, CPC, CPM, Fixed, RevShare

        public string Name { get; set; } // e.g. US - Casino Pack AT&T 3G/4G - Android Only

        public string Description { get; set; } //e.g. Fun and Easy to download Mobile Game

        public string ImageUrl { get; set; }

        [DisplayName("Payable Action")]
        public string PayableAction { get; set; } // e.g. Subscribe for 3-day free trial 

        public string Link { get; set; } // e.g. http://dvs.galaxy.gs/land.php?ms=6&key=76591de61b798d096940fae9e025634708f386e1|105&sid=

        public string CostCurrency { get; set; } // e.g. $

        [DisplayName("Cost (Payout)")]
        public decimal Cost { get; set; } // e.g. 1.85 

        public string RevenueCurrency { get; set; } // e.g. $

        [DisplayName("Revenue (Payout to Direct Agents)")]
        public decimal Revenue { get; set; } // e.g. 2.50

        public bool RevenueIsPercentage { get; set; }

        [DataType(DataType.MultilineText)]
        public string Restrictions { get; set; } // e.g. No search allowed

        [DisplayName("Important Details (from Wiki)")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string ImportantDetails { get; set; } // e.g. Important Details 
        //      Android Only 
        //      No content locking sites 
        //      No free movie download sites 

        public string Budget { get; set; } // e.g. 500 leads

        [DisplayName("Passed Information")]
        public string PassedInfo { get; set; } // e.g. Lead IDs

        [DisplayName("Lead Caps")]
        public string CampaignCap { get; set; } // e.g. 3,500 downloads per month 

        [DisplayName("Scrub Policy")]
        public string ScrubPolicy { get; set; } // e.g. No Scrub except fraud. 

        [DisplayName("EOM Notes")]
        public string EomNotes { get; set; } // e.g. EOM will be finalized in 5 days. 

//        [NotMapped]
//        [DisplayName("Mobile Allowed")]
//        public string MobileAllowed { get { return "y"; } set { } } // on hold

        [DisplayName("Mobile LP")]
        public string MobileLP { get; set; }

        public bool Hidden { get; set; }

        public int? StatusId { get; set; }
    }

    [MetadataType(typeof(Campaign_Metadata))]
    public partial class Campaign
    {
    }
}
