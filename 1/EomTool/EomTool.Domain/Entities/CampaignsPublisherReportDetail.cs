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
    
    public partial class CampaignsPublisherReportDetail
    {
        public string ItemIDs { get; set; }
        public string CampaignStatus { get; set; }
        public string Publisher { get; set; }
        public string AddCode { get; set; }
        public string CampaignName { get; set; }
        public Nullable<decimal> NumUnits { get; set; }
        public Nullable<decimal> CostPerUnit { get; set; }
        public string NetTerms { get; set; }
        public string PayCurrency { get; set; }
        public string IsCPM { get; set; }
        public string MediaBuyer { get; set; }
        public Nullable<decimal> ToBePaid { get; set; }
        public Nullable<decimal> Paid { get; set; }
        public Nullable<decimal> Total { get; set; }
    }
}