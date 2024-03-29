﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EomEntities : DbContext
    {
        public EomEntities()
            : base("name=EomEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Advertiser> Advertisers { get; set; }
        public virtual DbSet<Affiliate> Affiliates { get; set; }
        public virtual DbSet<Campaign> Campaigns { get; set; }
        public virtual DbSet<CampaignStatus> CampaignStatuses { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemAccountingStatus> ItemAccountingStatuses { get; set; }
        public virtual DbSet<MediaBuyer> MediaBuyers { get; set; }
        public virtual DbSet<NetTermType> NetTermTypes { get; set; }
        public virtual DbSet<Source> Sources { get; set; }
        public virtual DbSet<TrackingSystem> TrackingSystems { get; set; }
        public virtual DbSet<UnitType> UnitTypes { get; set; }
        public virtual DbSet<CampaignsPublisherReportDetail> CampaignsPublisherReportDetails { get; set; }
        public virtual DbSet<CampaignsPublisherReportSummary> CampaignsPublisherReportSummaries { get; set; }
        public virtual DbSet<PublisherReportDetail> PublisherReportDetails { get; set; }
        public virtual DbSet<PublisherReport> PublisherReports { get; set; }
        public virtual DbSet<PublisherReportSummary> PublisherReportSummaries { get; set; }
        public virtual DbSet<MediaBuyerApprovalStatus> MediaBuyerApprovalStatuses { get; set; }
        public virtual DbSet<PublisherPayout> PublisherPayouts { get; set; }
        public virtual DbSet<Batch> Batches { get; set; }
        public virtual DbSet<BatchUpdate> BatchUpdates { get; set; }
        public virtual DbSet<PaymentBatch> PaymentBatches { get; set; }
        public virtual DbSet<PaymentBatchState> PaymentBatchStates { get; set; }
        public virtual DbSet<PublisherPayment> PublisherPayments { get; set; }
        public virtual DbSet<PubAttachment> PubAttachments { get; set; }
        public virtual DbSet<PubNote> PubNotes { get; set; }
        public virtual DbSet<PublisherRelatedItemCount> PublisherRelatedItemCounts { get; set; }
        public virtual DbSet<AccountManager> AccountManagers { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceNote> InvoiceNotes { get; set; }
        public virtual DbSet<InvoiceStatus> InvoiceStatuses { get; set; }
        public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }
        public virtual DbSet<AffiliatePaymentMethod> AffiliatePaymentMethods { get; set; }
        public virtual DbSet<MarginApproval> MarginApprovals { get; set; }
        public virtual DbSet<AdManager> AdManagers { get; set; }
        public virtual DbSet<CampaignNote> CampaignNotes { get; set; }
        public virtual DbSet<Audit> Audits { get; set; }
        public virtual DbSet<AnalystRole> AnalystRoles { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<IncomeType> IncomeTypes { get; set; }
        public virtual DbSet<Analyst> Analysts { get; set; }
        public virtual DbSet<AnalystManager> AnalystManagers { get; set; }
        public virtual DbSet<Strategist> Strategists { get; set; }
        public virtual DbSet<CampAff> CampAffs { get; set; }
    }
}
