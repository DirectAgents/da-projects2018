namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerticalsEtc : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.Vertical",
                c => new
                    {
                        VerticalId = c.Int(nullable: false),
                        VerticalName = c.String(),
                    })
                .PrimaryKey(t => t.VerticalId);
            
            CreateTable(
                "cake.OfferType",
                c => new
                    {
                        OfferTypeId = c.Int(nullable: false),
                        OfferTypeName = c.String(),
                    })
                .PrimaryKey(t => t.OfferTypeId);
            
            CreateTable(
                "cake.OfferStatus",
                c => new
                    {
                        OfferStatusId = c.Int(nullable: false),
                        OfferStatusName = c.String(),
                    })
                .PrimaryKey(t => t.OfferStatusId);
            
            AddColumn("cake.Offer", "VerticalId", c => c.Int());
            AddColumn("cake.Offer", "OfferTypeId", c => c.Int());
            AddColumn("cake.Offer", "OfferStatusId", c => c.Int());
            AddColumn("cake.Offer", "Hidden", c => c.Boolean(nullable: false));
            AddForeignKey("cake.Offer", "VerticalId", "cake.Vertical", "VerticalId");
            AddForeignKey("cake.Offer", "OfferTypeId", "cake.OfferType", "OfferTypeId");
            AddForeignKey("cake.Offer", "OfferStatusId", "cake.OfferStatus", "OfferStatusId");
            CreateIndex("cake.Offer", "VerticalId");
            CreateIndex("cake.Offer", "OfferTypeId");
            CreateIndex("cake.Offer", "OfferStatusId");
        }
        
        public override void Down()
        {
            DropIndex("cake.Offer", new[] { "OfferStatusId" });
            DropIndex("cake.Offer", new[] { "OfferTypeId" });
            DropIndex("cake.Offer", new[] { "VerticalId" });
            DropForeignKey("cake.Offer", "OfferStatusId", "cake.OfferStatus");
            DropForeignKey("cake.Offer", "OfferTypeId", "cake.OfferType");
            DropForeignKey("cake.Offer", "VerticalId", "cake.Vertical");
            DropColumn("cake.Offer", "Hidden");
            DropColumn("cake.Offer", "OfferStatusId");
            DropColumn("cake.Offer", "OfferTypeId");
            DropColumn("cake.Offer", "VerticalId");
            DropTable("cake.OfferStatus");
            DropTable("cake.OfferType");
            DropTable("cake.Vertical");
        }
    }
}
