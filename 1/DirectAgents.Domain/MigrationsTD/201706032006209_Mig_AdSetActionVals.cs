namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdSetActionVals : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.AdSetAction", "PostClickVal", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("td.AdSetAction", "PostViewVal", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
            DropColumn("td.AdSetAction", "PostViewVal");
            DropColumn("td.AdSetAction", "PostClickVal");
        }
    }
}
