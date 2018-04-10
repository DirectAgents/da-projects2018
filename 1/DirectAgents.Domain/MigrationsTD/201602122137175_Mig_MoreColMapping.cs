namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_MoreColMapping : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.PlatColMapping", "StrategyName", c => c.String());
            AddColumn("td.PlatColMapping", "StrategyEid", c => c.String());
            AddColumn("td.PlatColMapping", "TDadName", c => c.String());
            AddColumn("td.PlatColMapping", "TDadEid", c => c.String());
            AddColumn("td.PlatColMapping", "SiteName", c => c.String());
            AddColumn("td.PlatColMapping", "Month", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.PlatColMapping", "Month");
            DropColumn("td.PlatColMapping", "SiteName");
            DropColumn("td.PlatColMapping", "TDadEid");
            DropColumn("td.PlatColMapping", "TDadName");
            DropColumn("td.PlatColMapping", "StrategyEid");
            DropColumn("td.PlatColMapping", "StrategyName");
        }
    }
}
