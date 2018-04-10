namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_PlatformAccountDS : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.Account",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlatformId = c.Int(nullable: false),
                        ExternalId = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Platform", t => t.PlatformId, cascadeDelete: true)
                .Index(t => t.PlatformId);
            
            CreateTable(
                "td.Platform",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(maxLength: 50),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "CodeIndex");
            
            CreateTable(
                "td.DailySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.AccountId })
                .ForeignKey("td.Account", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.DailySummary", "AccountId", "td.Account");
            DropForeignKey("td.Account", "PlatformId", "td.Platform");
            DropIndex("td.DailySummary", new[] { "AccountId" });
            DropIndex("td.Platform", "CodeIndex");
            DropIndex("td.Account", new[] { "PlatformId" });
            DropTable("td.DailySummary");
            DropTable("td.Platform");
            DropTable("td.Account");
        }
    }
}
