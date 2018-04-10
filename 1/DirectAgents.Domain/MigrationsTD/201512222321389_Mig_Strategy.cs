namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Strategy : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.Strategy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ExternalId = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Account", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "td.StrategySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        StrategyId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        PostClickConv = c.Int(nullable: false),
                        PostViewConv = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.StrategyId })
                .ForeignKey("td.Strategy", t => t.StrategyId, cascadeDelete: true)
                .Index(t => t.StrategyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.StrategySummary", "StrategyId", "td.Strategy");
            DropForeignKey("td.Strategy", "AccountId", "td.Account");
            DropIndex("td.StrategySummary", new[] { "StrategyId" });
            DropIndex("td.Strategy", new[] { "AccountId" });
            DropTable("td.StrategySummary");
            DropTable("td.Strategy");
        }
    }
}
