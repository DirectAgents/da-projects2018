namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBM_DaySumConvs : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.DailySummary", "PostClickConv", c => c.Int(nullable: false));
            AddColumn("td.DailySummary", "PostViewConv", c => c.Int(nullable: false));
            DropColumn("td.DailySummary", "Conversions");
        }
        
        public override void Down()
        {
            AddColumn("td.DailySummary", "Conversions", c => c.Int(nullable: false));
            DropColumn("td.DailySummary", "PostViewConv");
            DropColumn("td.DailySummary", "PostClickConv");
        }
    }
}
