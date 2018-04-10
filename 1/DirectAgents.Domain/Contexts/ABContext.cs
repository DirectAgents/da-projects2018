using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Domain.Contexts
{
    public class ABContext : DbContext
    {
        public const string abSchema = "ab";

        //? set CommandTimeout in constructor ?

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // --- AB ---
            modelBuilder.Entity<ABClient>().ToTable("Client", abSchema);
            modelBuilder.Entity<ClientBudget>().ToTable("ClientBudget", abSchema)
                .HasKey(x => new { x.ClientId, x.Date });
            modelBuilder.Entity<ClientAccount>().ToTable("ClientAccount", abSchema);
            modelBuilder.Entity<ClientPayment>().ToTable("ClientPayment", abSchema);
            modelBuilder.Entity<ClientPaymentBit>().ToTable("ClientPaymentBit", abSchema);
            modelBuilder.Entity<Job>().ToTable("Job", abSchema);
            modelBuilder.Entity<ABExtraItem>().ToTable("ExtraItem", abSchema);
            modelBuilder.Entity<ABVendor>().ToTable("Vendor", abSchema);
            modelBuilder.Entity<UnitType>().ToTable("UnitType", abSchema);
            modelBuilder.Entity<Payment>().ToTable("Payment", abSchema);
            modelBuilder.Entity<PaymentBit>().ToTable("PaymentBit", abSchema);
            modelBuilder.Entity<PaymentBit>().HasRequired(x => x.ClientAccount).WithMany().WillCascadeOnDelete(false);

            modelBuilder.Entity<ProtoCampaign>().ToTable("ProtoCampaign", abSchema);
            modelBuilder.Entity<ProtoPeriod>().ToTable("ProtoPeriod", abSchema);
            modelBuilder.Entity<ProtoPayment>().ToTable("ProtoPayment", abSchema);
            modelBuilder.Entity<ProtoPaymentBit>().ToTable("ProtoPaymentBit", abSchema);
            modelBuilder.Entity<ProtoInvoice>().ToTable("ProtoInvoice", abSchema);
            modelBuilder.Entity<ProtoInvoiceBit>().ToTable("ProtoInvoiceBit", abSchema);
            modelBuilder.Entity<ProtoSpendBit>().ToTable("ProtoSpendBit", abSchema);
            //---
            modelBuilder.Entity<Period>().ToTable("Period", abSchema);
            modelBuilder.Entity<SpendBucket>().ToTable("SpendBucket", abSchema);
            modelBuilder.Entity<SpendBit>().ToTable("SpendBit", abSchema);
            modelBuilder.Entity<Campaign>().ToTable("Campaign", abSchema);

            modelBuilder.Entity<ABClient>().Property(x => x.ExtCredit).HasPrecision(14, 2);
            modelBuilder.Entity<ABClient>().Property(x => x.IntCredit).HasPrecision(14, 2);
            modelBuilder.Entity<ClientBudget>().Property(x => x.Value).HasPrecision(14, 2);
            modelBuilder.Entity<ClientPaymentBit>().Property(x => x.Value).HasPrecision(14, 2);
            modelBuilder.Entity<ABExtraItem>().Property(x => x.Revenue).HasPrecision(14, 2);
            modelBuilder.Entity<ABExtraItem>().Property(x => x.Cost).HasPrecision(14, 2);
            modelBuilder.Entity<PaymentBit>().Property(x => x.Value).HasPrecision(14, 2);

            modelBuilder.Entity<ProtoPaymentBit>().Property(x => x.Value).HasPrecision(14, 2);
            modelBuilder.Entity<ProtoInvoiceBit>().Property(x => x.Value).HasPrecision(14, 2);
            modelBuilder.Entity<ProtoSpendBit>().Property(x => x.Revenue).HasPrecision(14, 2);
            //---
            modelBuilder.Entity<SpendBit>().Property(x => x.Revenue).HasPrecision(14, 2);
            modelBuilder.Entity<SpendBit>().Property(x => x.Rate).HasPrecision(14, 2);
            modelBuilder.Entity<SpendBit>().Property(x => x.Cost).HasPrecision(14, 2);
        }

        // --- AB ---
        public DbSet<ABClient> ABClients { get; set; }
        public DbSet<ClientBudget> ClientBudgets { get; set; }
        public DbSet<ClientAccount> ClientAccounts { get; set; }
        public DbSet<ClientPayment> ClientPayments { get; set; }
        public DbSet<ClientPaymentBit> ClientPaymentBits { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ABExtraItem> ABExtraItems { get; set; }
        public DbSet<ABVendor> ABVendors { get; set; }
        public DbSet<UnitType> UnitTypes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentBit> PaymentBits { get; set; }

        public DbSet<ProtoCampaign> ProtoCampaigns { get; set; }
        public DbSet<ProtoPeriod> ProtoPeriods { get; set; }
        public DbSet<ProtoPayment> ProtoPayments { get; set; }
        public DbSet<ProtoPaymentBit> ProtoPaymentBits { get; set; }
        public DbSet<ProtoInvoice> ProtoInvoices { get; set; }
        public DbSet<ProtoInvoiceBit> ProtoInvoiceBits { get; set; }
        public DbSet<ProtoSpendBit> ProtoSpendBits { get; set; }
        //---
        public DbSet<Period> Periods { get; set; }
        public DbSet<SpendBucket> SpendBuckets { get; set; }
        public DbSet<SpendBit> SpendBits { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
    }
}
