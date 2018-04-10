namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Revenue : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.DailySummary", "PostClickRev", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("td.DailySummary", "PostViewRev", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
            DropColumn("td.DailySummary", "PostViewRev");
            DropColumn("td.DailySummary", "PostClickRev");
        }
    }
}
