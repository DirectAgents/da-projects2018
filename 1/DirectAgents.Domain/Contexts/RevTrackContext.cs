using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.Contexts
{
    public class RevTrackContext : DbContext
    {
        public const string extSchema = "ext";
        public const string tblProgVendor = "ProgVendor";

        //? set CommandTimeout in constructor ?

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // --- Prog ---
            modelBuilder.Entity<ProgClient>().ToTable("ProgClient", extSchema);
            modelBuilder.Entity<ProgCampaign>().ToTable("ProgCampaign", extSchema);
            modelBuilder.Entity<ProgVendor>().ToTable(tblProgVendor, extSchema);
            modelBuilder.Entity<ProgBudgetInfo>().ToTable("ProgBudgetInfo", extSchema);
            modelBuilder.Entity<ProgVendorBudgetInfo>().ToTable("ProgVendorBudgetInfo", extSchema);
            modelBuilder.Entity<ProgSummary>().ToTable("ProgSummary", extSchema);
            modelBuilder.Entity<ProgExtraItem>().ToTable("ProgExtraItem", extSchema);

            modelBuilder.Entity<ProgCampaign>().Property(c => c.DefaultBudgetInfo.MediaSpend).HasPrecision(14, 2).HasColumnName("MediaSpend");
            modelBuilder.Entity<ProgCampaign>().Property(c => c.DefaultBudgetInfo.MgmtFeePct).HasPrecision(10, 5).HasColumnName("MgmtFeePct");
            modelBuilder.Entity<ProgCampaign>().Property(c => c.DefaultBudgetInfo.MarginPct).HasPrecision(10, 5).HasColumnName("MarginPct");
            modelBuilder.Entity<ProgBudgetInfo>().Property(b => b.MediaSpend).HasPrecision(14, 2);
            modelBuilder.Entity<ProgBudgetInfo>().Property(b => b.MgmtFeePct).HasPrecision(10, 5);
            modelBuilder.Entity<ProgBudgetInfo>().Property(b => b.MarginPct).HasPrecision(10, 5);
            modelBuilder.Entity<ProgBudgetInfo>()
                .HasKey(b => new { b.ProgCampaignId, b.Date });
            modelBuilder.Entity<ProgVendorBudgetInfo>().Property(b => b.MediaSpend).HasPrecision(14, 2);
            modelBuilder.Entity<ProgVendorBudgetInfo>().Property(b => b.MgmtFeePct).HasPrecision(10, 5);
            modelBuilder.Entity<ProgVendorBudgetInfo>().Property(b => b.MarginPct).HasPrecision(10, 5);
            modelBuilder.Entity<ProgVendorBudgetInfo>()
                .HasKey(b => new { b.ProgCampaignId, b.ProgVendorId, b.Date });
            modelBuilder.Entity<ProgSummary>()
                .HasKey(p => new { p.Date, p.ProgCampaignId, p.ProgVendorId })
                .Property(p => p.Cost).HasPrecision(18, 6);
            modelBuilder.Entity<ProgExtraItem>().Property(i => i.Cost).HasPrecision(14, 2);
            modelBuilder.Entity<ProgExtraItem>().Property(i => i.Revenue).HasPrecision(14, 2);
        }

        // --- Prog ---
        public DbSet<ProgClient> ProgClients { get; set; }
        public DbSet<ProgCampaign> ProgCampaigns { get; set; }
        public DbSet<ProgVendor> ProgVendors { get; set; }
        public DbSet<ProgBudgetInfo> ProgBudgetInfos { get; set; }
        public DbSet<ProgVendorBudgetInfo> ProgVendorBudgetInfos { get; set; }
        public DbSet<ProgSummary> ProgSummaries { get; set; }
        public DbSet<ProgExtraItem> ProgExtraItems { get; set; }
    }
}
