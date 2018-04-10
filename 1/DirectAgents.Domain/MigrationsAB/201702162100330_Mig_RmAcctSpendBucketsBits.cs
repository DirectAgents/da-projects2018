namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_RmAcctSpendBucketsBits : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ab.AcctSpendBit", "PeriodId", "ab.Period");
            DropForeignKey("ab.AcctSpendBit", "AcctSpendBucketId", "ab.AcctSpendBucket");
            DropForeignKey("ab.AcctSpendBucket", "CampaignId", "ab.Campaign");
            DropForeignKey("ab.AcctSpendBucket", "AcctId", "ab.ClientAccount");
            DropForeignKey("ab.AcctInvoiceBit", "AcctSpendBitId", "ab.AcctSpendBit");
            DropIndex("ab.AcctInvoiceBit", new[] { "AcctSpendBitId" });
            DropIndex("ab.AcctSpendBit", new[] { "PeriodId" });
            DropIndex("ab.AcctSpendBit", new[] { "AcctSpendBucketId" });
            DropIndex("ab.AcctSpendBucket", new[] { "AcctId" });
            DropIndex("ab.AcctSpendBucket", new[] { "CampaignId" });
            DropColumn("ab.AcctInvoiceBit", "AcctSpendBitId");
            DropTable("ab.AcctSpendBit");
            DropTable("ab.AcctSpendBucket");
        }
        
        public override void Down()
        {
            CreateTable(
                "ab.AcctSpendBucket",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        CampaignId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ab.AcctSpendBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        AcctSpendBucketId = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Desc = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("ab.AcctInvoiceBit", "AcctSpendBitId", c => c.Int());
            CreateIndex("ab.AcctSpendBucket", "CampaignId");
            CreateIndex("ab.AcctSpendBucket", "AcctId");
            CreateIndex("ab.AcctSpendBit", "AcctSpendBucketId");
            CreateIndex("ab.AcctSpendBit", "PeriodId");
            CreateIndex("ab.AcctInvoiceBit", "AcctSpendBitId");
            AddForeignKey("ab.AcctInvoiceBit", "AcctSpendBitId", "ab.AcctSpendBit", "Id");
            AddForeignKey("ab.AcctSpendBucket", "AcctId", "ab.ClientAccount", "Id", cascadeDelete: true);
            AddForeignKey("ab.AcctSpendBucket", "CampaignId", "ab.Campaign", "Id", cascadeDelete: true);
            AddForeignKey("ab.AcctSpendBit", "AcctSpendBucketId", "ab.AcctSpendBucket", "Id", cascadeDelete: true);
            AddForeignKey("ab.AcctSpendBit", "PeriodId", "ab.Period", "Id", cascadeDelete: true);
        }
    }
}
