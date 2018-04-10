namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdSetAction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.AdSetAction",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        AdSetId = c.Int(nullable: false),
                        ActionTypeId = c.Int(nullable: false),
                        PostClick = c.Int(nullable: false),
                        PostView = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Date, t.AdSetId, t.ActionTypeId })
                .ForeignKey("td.ActionType", t => t.ActionTypeId, cascadeDelete: true)
                .ForeignKey("td.AdSet", t => t.AdSetId, cascadeDelete: true)
                .Index(t => t.AdSetId)
                .Index(t => t.ActionTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.AdSetAction", "AdSetId", "td.AdSet");
            DropForeignKey("td.AdSetAction", "ActionTypeId", "td.ActionType");
            DropIndex("td.AdSetAction", new[] { "ActionTypeId" });
            DropIndex("td.AdSetAction", new[] { "AdSetId" });
            DropTable("td.AdSetAction");
        }
    }
}
