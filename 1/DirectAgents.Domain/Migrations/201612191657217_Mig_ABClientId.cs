namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ABClientId : DbMigration
    {
        public override void Up()
        {
            AddColumn("cake.Advertiser", "ABClientId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("cake.Advertiser", "ABClientId");
        }
    }
}
