namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ExtraItem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.ExtraItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        CampaignId = c.Int(nullable: false),
                        PlatformId = c.Int(nullable: false),
                        Description = c.String(),
                        Cost = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Campaign", t => t.CampaignId, cascadeDelete: true)
                .ForeignKey("td.Platform", t => t.PlatformId, cascadeDelete: true)
                .Index(t => t.CampaignId)
                .Index(t => t.PlatformId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.ExtraItem", "PlatformId", "td.Platform");
            DropForeignKey("td.ExtraItem", "CampaignId", "td.Campaign");
            DropIndex("td.ExtraItem", new[] { "PlatformId" });
            DropIndex("td.ExtraItem", new[] { "CampaignId" });
            DropTable("td.ExtraItem");
        }
    }
}
