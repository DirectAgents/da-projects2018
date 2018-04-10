namespace DirectAgents.Domain.Contexts
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DirectAgents.Domain.Entities.CPSearch;

    public partial class ClientPortalSearchContext : DbContext
    {
        public ClientPortalSearchContext()
            : base("name=ClientPortalSearchContext")
        {
            Database.SetInitializer<ClientPortalSearchContext>(null);
        }

        public virtual DbSet<SearchAccount> SearchAccounts { get; set; }
        public virtual DbSet<SearchCampaign> SearchCampaigns { get; set; }
        public virtual DbSet<SearchConvSummary> SearchConvSummaries { get; set; }
        public virtual DbSet<SearchConvType> SearchConvTypes { get; set; }
        public virtual DbSet<SearchDailySummary> SearchDailySummaries { get; set; }
        public virtual DbSet<SearchProfile> SearchProfiles { get; set; }
        public virtual DbSet<CallDailySummary> CallDailySummaries { get; set; }

        public virtual DbSet<ClientReport> ClientReports { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SearchAccount>()
                .Property(e => e.RevPerOrder)
                .HasPrecision(14, 2);

            modelBuilder.Entity<SearchAccount>()
                .HasMany(e => e.SearchCampaigns)
                .WithOptional(e => e.SearchAccount)
                .HasForeignKey(e => e.SearchAccountId);

            modelBuilder.Entity<SearchAccount>()
                .HasMany(e => e.AltSearchCampaigns)
                .WithOptional(e => e.AltSearchAccount)
                .HasForeignKey(e => e.AltSearchAccountId);

            modelBuilder.Entity<SearchCampaign>()
                .HasMany(e => e.SearchConvSummaries)
                .WithRequired(e => e.SearchCampaign)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SearchCampaign>()
                .HasMany(e => e.SearchDailySummaries)
                .WithRequired(e => e.SearchCampaign)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SearchConvSummary>()
                .Property(e => e.Network)
                .IsFixedLength();

            modelBuilder.Entity<SearchConvSummary>()
                .Property(e => e.Device)
                .IsFixedLength();

            modelBuilder.Entity<SearchConvSummary>()
                .Property(e => e.ConVal)
                .HasPrecision(18, 6);

            //modelBuilder.Entity<SearchConvType>()
            //    .HasMany(e => e.SearchConvSummaries)
            //    .WithRequired(e => e.SearchConvType)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<SearchDailySummary>()
                .Property(e => e.Revenue)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SearchDailySummary>()
                .Property(e => e.Cost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<SearchDailySummary>()
                .Property(e => e.Network)
                .IsFixedLength();

            modelBuilder.Entity<SearchDailySummary>()
                .Property(e => e.Device)
                .IsFixedLength();
        }
    }
}
