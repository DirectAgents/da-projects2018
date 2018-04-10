namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_OfferContract : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.OfferContract",
                c => new
                    {
                        OfferContractId = c.Int(nullable: false),
                        OfferId = c.Int(nullable: false),
                        PriceFormatName = c.String(),
                        ReceivedAmount = c.Decimal(nullable: false, precision: 19, scale: 4),
                    })
                .PrimaryKey(t => t.OfferContractId)
                .ForeignKey("cake.Offer", t => t.OfferId, cascadeDelete: true)
                .Index(t => t.OfferId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("cake.OfferContract", "OfferId", "cake.Offer");
            DropIndex("cake.OfferContract", new[] { "OfferId" });
            DropTable("cake.OfferContract");
        }
    }
}
