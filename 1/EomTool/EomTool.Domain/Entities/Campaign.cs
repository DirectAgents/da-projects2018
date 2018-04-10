//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EomTool.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Campaign
    {
        public int id { get; set; }
        public int account_manager_id { get; set; }
        public int campaign_status_id { get; set; }
        public int ad_manager_id { get; set; }
        public int advertiser_id { get; set; }
        public int pid { get; set; }
        public string campaign_name { get; set; }
        public string campaign_type { get; set; }
        public Nullable<System.DateTime> modified { get; set; }
        public Nullable<System.DateTime> created { get; set; }
        public string dt_campaign_status { get; set; }
        public string dt_campaign_url { get; set; }
        public string dt_allowed_country_names { get; set; }
        public Nullable<bool> is_email { get; set; }
        public Nullable<bool> is_search { get; set; }
        public Nullable<bool> is_display { get; set; }
        public Nullable<bool> is_coreg { get; set; }
        public Nullable<int> max_scrub { get; set; }
        public string notes { get; set; }
        public Nullable<int> tracking_system_id { get; set; }
        public Nullable<int> external_id { get; set; }
        public string display_name { get; set; }
    
        public virtual Advertiser Advertiser { get; set; }
        public virtual CampaignStatus CampaignStatus { get; set; }
        public virtual TrackingSystem TrackingSystem { get; set; }
        public virtual AccountManager AccountManager { get; set; }
        public virtual AdManager AdManager { get; set; }
    }
}