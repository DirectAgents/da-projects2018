namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdvCampBudg : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.Campaign",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdvertiserId = c.Int(nullable: false),
                        Name = c.String(),
                        MediaSpend = c.Decimal(nullable: false, precision: 14, scale: 2),
                        MgmtFeePct = c.Decimal(nullable: false, precision: 8, scale: 3),
                        MarginPct = c.Decimal(nullable: false, precision: 8, scale: 3),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Advertiser", t => t.AdvertiserId, cascadeDelete: true)
                .Index(t => t.AdvertiserId);
            
            CreateTable(
                "td.Advertiser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "td.BudgetInfo",
                c => new
                    {
                        CampaignId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        MediaSpend = c.Decimal(nullable: false, precision: 14, scale: 2),
                        MgmtFeePct = c.Decimal(nullable: false, precision: 8, scale: 3),
                        MarginPct = c.Decimal(nullable: false, precision: 8, scale: 3),
                    })
                .PrimaryKey(t => new { t.CampaignId, t.Date })
                .ForeignKey("td.Campaign", t => t.CampaignId, cascadeDelete: true)
                .Index(t => t.CampaignId);
            
            AddColumn("td.Account", "CampaignId", c => c.Int());
            CreateIndex("td.Account", "CampaignId");
            AddForeignKey("td.Account", "CampaignId", "td.Campaign", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("td.BudgetInfo", "CampaignId", "td.Campaign");
            DropForeignKey("td.Campaign", "AdvertiserId", "td.Advertiser");
            DropForeignKey("td.Account", "CampaignId", "td.Campaign");
            DropIndex("td.BudgetInfo", new[] { "CampaignId" });
            DropIndex("td.Campaign", new[] { "AdvertiserId" });
            DropIndex("td.Account", new[] { "CampaignId" });
            DropColumn("td.Account", "CampaignId");
            DropTable("td.BudgetInfo");
            DropTable("td.Advertiser");
            DropTable("td.Campaign");
        }
    }
}
