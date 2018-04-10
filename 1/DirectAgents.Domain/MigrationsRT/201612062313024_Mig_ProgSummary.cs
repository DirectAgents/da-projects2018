namespace DirectAgents.Domain.MigrationsRT
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ProgSummary : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ext.ProgExtraItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        ProgCampaignId = c.Int(nullable: false),
                        ProgVendorId = c.Int(nullable: false),
                        Description = c.String(),
                        Cost = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ext.ProgCampaign", t => t.ProgCampaignId, cascadeDelete: true)
                .ForeignKey("ext.ProgVendor", t => t.ProgVendorId, cascadeDelete: true)
                .Index(t => t.ProgCampaignId)
                .Index(t => t.ProgVendorId);
            
            CreateTable(
                "ext.ProgSummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        ProgCampaignId = c.Int(nullable: false),
                        ProgVendorId = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.ProgCampaignId, t.ProgVendorId })
                .ForeignKey("ext.ProgCampaign", t => t.ProgCampaignId, cascadeDelete: true)
                .ForeignKey("ext.ProgVendor", t => t.ProgVendorId, cascadeDelete: true)
                .Index(t => t.ProgCampaignId)
                .Index(t => t.ProgVendorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ext.ProgSummary", "ProgVendorId", "ext.ProgVendor");
            DropForeignKey("ext.ProgSummary", "ProgCampaignId", "ext.ProgCampaign");
            DropForeignKey("ext.ProgExtraItem", "ProgVendorId", "ext.ProgVendor");
            DropForeignKey("ext.ProgExtraItem", "ProgCampaignId", "ext.ProgCampaign");
            DropIndex("ext.ProgSummary", new[] { "ProgVendorId" });
            DropIndex("ext.ProgSummary", new[] { "ProgCampaignId" });
            DropIndex("ext.ProgExtraItem", new[] { "ProgVendorId" });
            DropIndex("ext.ProgExtraItem", new[] { "ProgCampaignId" });
            DropTable("ext.ProgSummary");
            DropTable("ext.ProgExtraItem");
        }
    }
}
