namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdDestinationUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Ad", "DestinationUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.Ad", "DestinationUrl");
        }
    }
}
