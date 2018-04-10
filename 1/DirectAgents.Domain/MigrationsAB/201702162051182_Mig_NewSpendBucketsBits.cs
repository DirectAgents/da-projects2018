namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_NewSpendBucketsBits : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.SpendBucket",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        CampaignId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.AcctId, cascadeDelete: true)
                .ForeignKey("ab.Campaign", t => t.CampaignId, cascadeDelete: true)
                .Index(t => t.AcctId)
                .Index(t => t.CampaignId);
            
            CreateTable(
                "ab.SpendBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        SpendBucketId = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Quantity = c.Int(nullable: false),
                        Rate = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Cost = c.Decimal(nullable: false, precision: 14, scale: 2),
                        IsFee = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Period", t => t.PeriodId, cascadeDelete: true)
                .ForeignKey("ab.SpendBucket", t => t.SpendBucketId, cascadeDelete: true)
                .Index(t => t.PeriodId)
                .Index(t => t.SpendBucketId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.SpendBit", "SpendBucketId", "ab.SpendBucket");
            DropForeignKey("ab.SpendBit", "PeriodId", "ab.Period");
            DropForeignKey("ab.SpendBucket", "CampaignId", "ab.Campaign");
            DropForeignKey("ab.SpendBucket", "AcctId", "ab.ClientAccount");
            DropIndex("ab.SpendBit", new[] { "SpendBucketId" });
            DropIndex("ab.SpendBit", new[] { "PeriodId" });
            DropIndex("ab.SpendBucket", new[] { "CampaignId" });
            DropIndex("ab.SpendBucket", new[] { "AcctId" });
            DropTable("ab.SpendBit");
            DropTable("ab.SpendBucket");
        }
    }
}
