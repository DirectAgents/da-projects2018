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
    
    public partial class PublisherPayment
    {
        public Nullable<int> NetTermTypeId { get; set; }
        public string NetTermType { get; set; }
        public string AffIds { get; set; }
        public string Publisher { get; set; }
        public string PubPayCurr { get; set; }
        public Nullable<decimal> PubPayout { get; set; }
        public int AccountingStatusId { get; set; }
        public string AccountingStatus { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; }
        public Nullable<int> PaymentBatchId { get; set; }
        public int PaymentBatchStateId { get; set; }
        public string PaymentBatchState { get; set; }
        public string ApproverIdentity { get; set; }
        public string ItemIds { get; set; }
    }
}
