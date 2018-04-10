namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OfferUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("cake.Offer", "DateCreated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("cake.Offer", "DateCreated");
        }
    }
}
