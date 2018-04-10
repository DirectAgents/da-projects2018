namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdvUpdates : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Advertiser", "Logo", c => c.Binary());
            AddColumn("td.Advertiser", "StartDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("td.Advertiser", "StartDate");
            DropColumn("td.Advertiser", "Logo");
        }
    }
}
