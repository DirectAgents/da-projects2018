namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Actions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.ActionType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(maxLength: 100),
                        DisplayName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true, name: "CodeIndex");
            
            CreateTable(
                "td.StrategyAction",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        StrategyId = c.Int(nullable: false),
                        ActionTypeId = c.Int(nullable: false),
                        PostClick = c.Int(nullable: false),
                        PostView = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Date, t.StrategyId, t.ActionTypeId })
                .ForeignKey("td.ActionType", t => t.ActionTypeId, cascadeDelete: true)
                .ForeignKey("td.Strategy", t => t.StrategyId, cascadeDelete: true)
                .Index(t => t.StrategyId)
                .Index(t => t.ActionTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.StrategyAction", "StrategyId", "td.Strategy");
            DropForeignKey("td.StrategyAction", "ActionTypeId", "td.ActionType");
            DropIndex("td.StrategyAction", new[] { "ActionTypeId" });
            DropIndex("td.StrategyAction", new[] { "StrategyId" });
            DropIndex("td.ActionType", "CodeIndex");
            DropTable("td.StrategyAction");
            DropTable("td.ActionType");
        }
    }
}
