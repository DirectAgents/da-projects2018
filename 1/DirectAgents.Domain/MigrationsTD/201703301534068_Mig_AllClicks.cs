namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AllClicks : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.AdSetSummary", "AllClicks", c => c.Int(nullable: false));
            AddColumn("td.DailySummary", "AllClicks", c => c.Int(nullable: false));
            AddColumn("td.SiteSummary", "AllClicks", c => c.Int(nullable: false));
            AddColumn("td.StrategySummary", "AllClicks", c => c.Int(nullable: false));
            AddColumn("td.AdSummary", "AllClicks", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("td.AdSummary", "AllClicks");
            DropColumn("td.StrategySummary", "AllClicks");
            DropColumn("td.SiteSummary", "AllClicks");
            DropColumn("td.DailySummary", "AllClicks");
            DropColumn("td.AdSetSummary", "AllClicks");
        }
    }
}
