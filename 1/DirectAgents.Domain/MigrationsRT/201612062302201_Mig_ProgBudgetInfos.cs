namespace DirectAgents.Domain.MigrationsRT
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ProgBudgetInfos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ext.ProgBudgetInfo",
                c => new
                    {
                        ProgCampaignId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        MediaSpend = c.Decimal(nullable: false, precision: 14, scale: 2),
                        MgmtFeePct = c.Decimal(nullable: false, precision: 10, scale: 5),
                        MarginPct = c.Decimal(nullable: false, precision: 10, scale: 5),
                    })
                .PrimaryKey(t => new { t.ProgCampaignId, t.Date })
                .ForeignKey("ext.ProgCampaign", t => t.ProgCampaignId, cascadeDelete: true)
                .Index(t => t.ProgCampaignId);
            
            CreateTable(
                "ext.ProgVendorBudgetInfo",
                c => new
                    {
                        ProgCampaignId = c.Int(nullable: false),
                        ProgVendorId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        MediaSpend = c.Decimal(nullable: false, precision: 14, scale: 2),
                        MgmtFeePct = c.Decimal(nullable: false, precision: 10, scale: 5),
                        MarginPct = c.Decimal(nullable: false, precision: 10, scale: 5),
                    })
                .PrimaryKey(t => new { t.ProgCampaignId, t.ProgVendorId, t.Date })
                .ForeignKey("ext.ProgCampaign", t => t.ProgCampaignId, cascadeDelete: true)
                .ForeignKey("ext.ProgVendor", t => t.ProgVendorId, cascadeDelete: true)
                .Index(t => t.ProgCampaignId)
                .Index(t => t.ProgVendorId);
            
            AddColumn("ext.ProgCampaign", "MediaSpend", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            AddColumn("ext.ProgCampaign", "MgmtFeePct", c => c.Decimal(nullable: false, precision: 10, scale: 5));
            AddColumn("ext.ProgCampaign", "MarginPct", c => c.Decimal(nullable: false, precision: 10, scale: 5));
        }
        
        public override void Down()
        {
            DropForeignKey("ext.ProgVendorBudgetInfo", "ProgVendorId", "ext.ProgVendor");
            DropForeignKey("ext.ProgVendorBudgetInfo", "ProgCampaignId", "ext.ProgCampaign");
            DropForeignKey("ext.ProgBudgetInfo", "ProgCampaignId", "ext.ProgCampaign");
            DropIndex("ext.ProgVendorBudgetInfo", new[] { "ProgVendorId" });
            DropIndex("ext.ProgVendorBudgetInfo", new[] { "ProgCampaignId" });
            DropIndex("ext.ProgBudgetInfo", new[] { "ProgCampaignId" });
            DropColumn("ext.ProgCampaign", "MarginPct");
            DropColumn("ext.ProgCampaign", "MgmtFeePct");
            DropColumn("ext.ProgCampaign", "MediaSpend");
            DropTable("ext.ProgVendorBudgetInfo");
            DropTable("ext.ProgBudgetInfo");
        }
    }
}
