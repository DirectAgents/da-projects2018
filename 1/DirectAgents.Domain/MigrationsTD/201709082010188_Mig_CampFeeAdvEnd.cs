namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_CampFeeAdvEnd : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Campaign", "BaseFee", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            AddColumn("td.Advertiser", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("td.Advertiser", "EndDate");
            DropColumn("td.Campaign", "BaseFee");
        }
    }
}
