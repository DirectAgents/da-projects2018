namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Camp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.Camp",
                c => new
                    {
                        CampaignId = c.Int(nullable: false),
                        AffiliateId = c.Int(nullable: false),
                        OfferId = c.Int(nullable: false),
                        OfferContractId = c.Int(nullable: false),
                        PayoutAmount = c.Decimal(nullable: false, precision: 19, scale: 4),
                        CurrencyAbbr = c.String(),
                    })
                .PrimaryKey(t => t.CampaignId);
            
        }
        
        public override void Down()
        {
            DropTable("cake.Camp");
        }
    }
}
