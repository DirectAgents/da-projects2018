namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_BudgInfo_Id : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.BudgetInfo", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("td.PlatformBudgetInfo", "Id", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            DropColumn("td.PlatformBudgetInfo", "Id");
            DropColumn("td.BudgetInfo", "Id");
        }
    }
}
