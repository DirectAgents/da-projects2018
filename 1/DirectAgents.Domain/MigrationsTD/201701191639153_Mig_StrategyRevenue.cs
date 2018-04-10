namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_StrategyRevenue : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.StrategySummary", "PostClickRev", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("td.StrategySummary", "PostViewRev", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
            DropColumn("td.StrategySummary", "PostViewRev");
            DropColumn("td.StrategySummary", "PostClickRev");
        }
    }
}
