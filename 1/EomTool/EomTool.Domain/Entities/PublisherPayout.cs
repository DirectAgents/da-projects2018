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
    
    public partial class PublisherPayout
    {
        public int affid { get; set; }
        public string Publisher { get; set; }
        public string Advertiser { get; set; }
        public int pid { get; set; }
        public string Campaign_Name { get; set; }
        public string Rev_Currency { get; set; }
        public string Cost_Currency { get; set; }
        public decimal Rev_Unit { get; set; }
        public Nullable<decimal> Rev_Unit_USD { get; set; }
        public decimal Cost_Unit { get; set; }
        public Nullable<decimal> Cost_Unit_USD { get; set; }
        public Nullable<decimal> Units { get; set; }
        public string Unit_Type { get; set; }
        public Nullable<decimal> Revenue { get; set; }
        public Nullable<decimal> Revenue_USD { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public Nullable<decimal> Cost_USD { get; set; }
        public Nullable<decimal> Margin { get; set; }
        public Nullable<decimal> MarginPct { get; set; }
        public string Media_Buyer { get; set; }
        public string Ad_Manager { get; set; }
        public string Account_Manager { get; set; }
        public int status_id { get; set; }
        public string Status { get; set; }
        public int accounting_status_id { get; set; }
        public string Accounting_Status { get; set; }
        public int media_buyer_approval_status_id { get; set; }
        public string Media_Buyer_Approval_Status { get; set; }
        public string Net_Terms { get; set; }
        public string Aff_Pay_Method { get; set; }
        public string Pub_Pay_Curr { get; set; }
        public Nullable<decimal> Pub_Payout { get; set; }
        public string CampaignNotes { get; set; }
        public string Source { get; set; }
        public string ItemIds { get; set; }
        public string BatchIds { get; set; }
    }
}