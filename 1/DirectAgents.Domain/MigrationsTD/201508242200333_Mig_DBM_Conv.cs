namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBM_Conv : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbm.CreativeDailySummary", "PostClickConv", c => c.Int(nullable: false));
            AddColumn("dbm.CreativeDailySummary", "PostViewConv", c => c.Int(nullable: false));
            DropColumn("dbm.CreativeDailySummary", "Conversions");
        }
        
        public override void Down()
        {
            AddColumn("dbm.CreativeDailySummary", "Conversions", c => c.Int(nullable: false));
            DropColumn("dbm.CreativeDailySummary", "PostViewConv");
            DropColumn("dbm.CreativeDailySummary", "PostClickConv");
        }
    }
}
