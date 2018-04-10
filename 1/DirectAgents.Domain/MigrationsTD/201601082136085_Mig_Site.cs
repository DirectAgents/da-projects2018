namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Site : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.Site",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "td.SiteSummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        SiteId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        PostClickConv = c.Int(nullable: false),
                        PostViewConv = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.SiteId, t.AccountId })
                .ForeignKey("td.Account", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("td.Site", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.SiteSummary", "SiteId", "td.Site");
            DropForeignKey("td.SiteSummary", "AccountId", "td.Account");
            DropIndex("td.SiteSummary", new[] { "AccountId" });
            DropIndex("td.SiteSummary", new[] { "SiteId" });
            DropTable("td.SiteSummary");
            DropTable("td.Site");
        }
    }
}
