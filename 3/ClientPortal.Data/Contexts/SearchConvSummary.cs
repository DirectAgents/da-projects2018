//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClientPortal.Data.Contexts
{
    using System;
    using System.Collections.Generic;
    
    public partial class SearchConvSummary
    {
        public int SearchCampaignId { get; set; }
        public System.DateTime Date { get; set; }
        public int SearchConvTypeId { get; set; }
        public string Network { get; set; }
        public string Device { get; set; }
        public double Conversions { get; set; }
        public decimal ConVal { get; set; }
    
        public virtual SearchCampaign SearchCampaign { get; set; }
        public virtual SearchConvType SearchConvType { get; set; }
    }
}
