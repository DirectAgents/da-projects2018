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
    
    public partial class Item
    {
        public int id { get; set; }
        public string name { get; set; }
        public int pid { get; set; }
        public int affid { get; set; }
        public int source_id { get; set; }
        public int unit_type_id { get; set; }
        public Nullable<int> stat_id_n { get; set; }
        public int revenue_currency_id { get; set; }
        public int cost_currency_id { get; set; }
        public decimal revenue_per_unit { get; set; }
        public decimal cost_per_unit { get; set; }
        public decimal num_units { get; set; }
        public string notes { get; set; }
        public string accounting_notes { get; set; }
        public int item_accounting_status_id { get; set; }
        public int item_reporting_status_id { get; set; }
        public Nullable<decimal> total_revenue { get; set; }
        public Nullable<decimal> total_cost { get; set; }
        public Nullable<decimal> margin { get; set; }
        public System.DateTime modified { get; set; }
        public int campaign_status_id { get; set; }
        public int media_buyer_approval_status_id { get; set; }
        public Nullable<int> batch_id { get; set; }
        public Nullable<int> payment_batch_id { get; set; }
    
        public virtual Currency RevenueCurrency { get; set; }
        public virtual Currency CostCurrency { get; set; }
        public virtual ItemAccountingStatus ItemAccountingStatus { get; set; }
        public virtual Source Source { get; set; }
        public virtual UnitType UnitType { get; set; }
        public virtual MediaBuyerApprovalStatus MediaBuyerApprovalStatus { get; set; }
        public virtual Batch Batch { get; set; }
        public virtual CampaignStatus CampaignStatus { get; set; }
        public virtual PaymentBatch PaymentBatch { get; set; }
    }
}
