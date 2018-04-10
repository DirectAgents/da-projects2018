namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_PlatformBudgetInfo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.PlatformBudgetInfo",
                c => new
                    {
                        CampaignId = c.Int(nullable: false),
                        PlatformId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        MediaSpend = c.Decimal(nullable: false, precision: 14, scale: 2),
                        MgmtFeePct = c.Decimal(nullable: false, precision: 10, scale: 5),
                        MarginPct = c.Decimal(nullable: false, precision: 10, scale: 5),
                    })
                .PrimaryKey(t => new { t.CampaignId, t.PlatformId, t.Date })
                .ForeignKey("td.Campaign", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("td.Platform", t => t.PlatformId, cascadeDelete: true)
                .Index(t => t.CampaignId)
                .Index(t => t.PlatformId);
            
            AlterColumn("td.Campaign", "MgmtFeePct", c => c.Decimal(nullable: false, precision: 10, scale: 5));
            AlterColumn("td.Campaign", "MarginPct", c => c.Decimal(nullable: false, precision: 10, scale: 5));
            AlterColumn("td.BudgetInfo", "MgmtFeePct", c => c.Decimal(nullable: false, precision: 10, scale: 5));
            AlterColumn("td.BudgetInfo", "MarginPct", c => c.Decimal(nullable: false, precision: 10, scale: 5));
        }
        
        public override void Down()
        {
            DropForeignKey("td.PlatformBudgetInfo", "PlatformId", "td.Platform");
            DropForeignKey("td.PlatformBudgetInfo", "CampaignId", "td.Campaign");
            DropIndex("td.PlatformBudgetInfo", new[] { "PlatformId" });
            DropIndex("td.PlatformBudgetInfo", new[] { "CampaignId" });
            AlterColumn("td.BudgetInfo", "MarginPct", c => c.Decimal(nullable: false, precision: 8, scale: 3));
            AlterColumn("td.BudgetInfo", "MgmtFeePct", c => c.Decimal(nullable: false, precision: 8, scale: 3));
            AlterColumn("td.Campaign", "MarginPct", c => c.Decimal(nullable: false, precision: 8, scale: 3));
            AlterColumn("td.Campaign", "MgmtFeePct", c => c.Decimal(nullable: false, precision: 8, scale: 3));
            DropTable("td.PlatformBudgetInfo");
        }
    }
}
