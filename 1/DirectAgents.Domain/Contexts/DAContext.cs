using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DirectAgents.Domain.Entities;
using DirectAgents.Domain.Entities.AdRoll;
using DirectAgents.Domain.Entities.Cake;
using DirectAgents.Domain.Entities.Screen;

namespace DirectAgents.Domain.Contexts
{
    public class DAContext : DbContext
    {
        //public DAContext() : base() { }
        const string cakeSchema = "cake";
        const string screenSchema = "screen";

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Cake
            modelBuilder.Entity<Advertiser>().ToTable("Advertiser", cakeSchema);
            modelBuilder.Entity<Affiliate>().ToTable("Affiliate", cakeSchema);
            modelBuilder.Entity<Contact>().ToTable("Contact", cakeSchema);
            modelBuilder.Entity<Role>().ToTable("Role", cakeSchema);
            modelBuilder.Entity<Offer>().ToTable("Offer", cakeSchema);
            modelBuilder.Entity<Vertical>().ToTable("Vertical", cakeSchema);
            modelBuilder.Entity<OfferType>().ToTable("OfferType", cakeSchema);
            modelBuilder.Entity<OfferStatus>().ToTable("OfferStatus", cakeSchema);
            modelBuilder.Entity<OfferContract>().ToTable("OfferContract", cakeSchema);
            modelBuilder.Entity<Camp>().ToTable("Camp", cakeSchema);
            modelBuilder.Entity<OfferDailySummary>()
                .HasKey(t => new { t.OfferId, t.Date }).ToTable("OfferDailySummary", cakeSchema);
            modelBuilder.Entity<CampSum>()
                .HasKey(x => new { x.CampId, x.Date }).ToTable("CampSum", cakeSchema);
            modelBuilder.Entity<CampSum>().HasRequired(x => x.CostCurr).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<CampSum>().HasRequired(x => x.RevCurr).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Event>().ToTable("Event", cakeSchema);
            modelBuilder.Entity<PriceFormat>().ToTable("PriceFormat", cakeSchema);
            modelBuilder.Entity<EventConversion>().ToTable("EventConversion", cakeSchema);
            modelBuilder.Entity<EventConversion>().HasRequired(x => x.ReceivedCurr).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<EventConversion>().HasRequired(x => x.PaidCurr).WithMany().WillCascadeOnDelete(false);

            modelBuilder.Entity<OfferContract>().Property(x => x.ReceivedAmount).HasPrecision(19, 4);
            modelBuilder.Entity<Camp>().Property(x => x.PayoutAmount).HasPrecision(19, 4);
            modelBuilder.Entity<CampSum>().Property(x => x.Conversions).HasPrecision(16, 6);
            modelBuilder.Entity<CampSum>().Property(x => x.Paid).HasPrecision(16, 6);
            modelBuilder.Entity<CampSum>().Property(x => x.Sellable).HasPrecision(16, 6);
            modelBuilder.Entity<CampSum>().Property(x => x.Revenue).HasPrecision(19, 4);
            modelBuilder.Entity<CampSum>().Property(x => x.Cost).HasPrecision(19, 4);
            modelBuilder.Entity<CampSum>().Property(x => x.RevenuePerUnit).HasPrecision(19, 4);
            modelBuilder.Entity<CampSum>().Property(x => x.CostPerUnit).HasPrecision(19, 4);
            modelBuilder.Entity<EventConversion>().Property(x => x.Received).HasPrecision(19, 4);
            modelBuilder.Entity<EventConversion>().Property(x => x.Paid).HasPrecision(19, 4);

            // Screen
            modelBuilder.Entity<Salesperson>().ToTable("Salesperson", screenSchema);
            modelBuilder.Entity<SalespersonStat>()
                .HasKey(t => new { t.Date, t.SalespersonId })
                .ToTable("SalespersonStat", screenSchema);

            // general
            modelBuilder.Entity<Variable>()
                .Property(t => t.DecVal).HasPrecision(18, 6);
        }

        public DbSet<Advertiser> Advertisers { get; set; }
        public DbSet<Affiliate> Affiliates { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Vertical> Verticals { get; set; }
        public DbSet<OfferType> OfferTypes { get; set; }
        public DbSet<OfferStatus> OfferStatuses { get; set; }
        public DbSet<OfferContract> OfferContracts { get; set; }
        public DbSet<Camp> Camps { get; set; }
        public DbSet<OfferDailySummary> OfferDailySummaries { get; set; }
        public DbSet<CampSum> CampSums { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<PriceFormat> PriceFormats { get; set; }
        public DbSet<EventConversion> EventConversions { get; set; }

        public DbSet<Salesperson> Salespeople { get; set; }
        public DbSet<SalespersonStat> SalespersonStats { get; set; }

        public DbSet<OfferBudget> OfferBudgets { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public DbSet<Currency> Currencies { get; set; }
    }
}
