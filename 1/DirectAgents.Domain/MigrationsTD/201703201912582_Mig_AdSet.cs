namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.AdSet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        StrategyId = c.Int(),
                        ExternalId = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Account", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("td.Strategy", t => t.StrategyId)
                .Index(t => t.AccountId)
                .Index(t => t.StrategyId);
            
            CreateTable(
                "td.AdSetSummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        AdSetId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        PostClickConv = c.Int(nullable: false),
                        PostViewConv = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                        PostClickRev = c.Decimal(nullable: false, precision: 18, scale: 4),
                        PostViewRev = c.Decimal(nullable: false, precision: 18, scale: 4),
                    })
                .PrimaryKey(t => new { t.Date, t.AdSetId })
                .ForeignKey("td.AdSet", t => t.AdSetId, cascadeDelete: true)
                .Index(t => t.AdSetId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.AdSetSummary", "AdSetId", "td.AdSet");
            DropForeignKey("td.AdSet", "StrategyId", "td.Strategy");
            DropForeignKey("td.AdSet", "AccountId", "td.Account");
            DropIndex("td.AdSetSummary", new[] { "AdSetId" });
            DropIndex("td.AdSet", new[] { "StrategyId" });
            DropIndex("td.AdSet", new[] { "AccountId" });
            DropTable("td.AdSetSummary");
            DropTable("td.AdSet");
        }
    }
}
