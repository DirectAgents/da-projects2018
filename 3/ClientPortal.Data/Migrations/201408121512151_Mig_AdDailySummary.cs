namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdDailySummary : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "adr.AdRollProfile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TradingDeskAccountId = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TradingDeskAccount", t => t.TradingDeskAccountId)
                .Index(t => t.TradingDeskAccountId);
            
            CreateTable(
                "adr.AdRollAd",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdRollProfileId = c.Int(nullable: false),
                        Name = c.String(),
                        Size = c.String(),
                        Type = c.String(),
                        CreatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("adr.AdRollProfile", t => t.AdRollProfileId, cascadeDelete: true)
                .Index(t => t.AdRollProfileId);
            
            CreateTable(
                "adr.AdDailySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        AdRollAdId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Spend = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.Date, t.AdRollAdId })
                .ForeignKey("adr.AdRollAd", t => t.AdRollAdId, cascadeDelete: true)
                .Index(t => t.AdRollAdId);
            
        }
        
        public override void Down()
        {
            DropIndex("adr.AdDailySummary", new[] { "AdRollAdId" });
            DropIndex("adr.AdRollAd", new[] { "AdRollProfileId" });
            DropIndex("adr.AdRollProfile", new[] { "TradingDeskAccountId" });
            DropForeignKey("adr.AdDailySummary", "AdRollAdId", "adr.AdRollAd");
            DropForeignKey("adr.AdRollAd", "AdRollProfileId", "adr.AdRollProfile");
            DropForeignKey("adr.AdRollProfile", "TradingDeskAccountId", "dbo.TradingDeskAccount");
            DropTable("adr.AdDailySummary");
            DropTable("adr.AdRollAd");
            DropTable("adr.AdRollProfile");
        }
    }
}
