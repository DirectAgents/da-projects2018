namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OfferInfo",
                c => new
                    {
                        OfferId = c.Int(nullable: false),
                        Budget = c.Decimal(precision: 18, scale: 2),
                        BudgetIsMonthly = c.Boolean(nullable: false),
                        BudgetStart = c.DateTime(),
                    })
                .PrimaryKey(t => t.OfferId)
                .ForeignKey("cake.Offer", t => t.OfferId)
                .Index(t => t.OfferId);
            
            CreateTable(
                "cake.Advertiser",
                c => new
                    {
                        AdvertiserId = c.Int(nullable: false),
                        AdvertiserName = c.String(),
                    })
                .PrimaryKey(t => t.AdvertiserId);
            
            CreateTable(
                "cake.Offer",
                c => new
                    {
                        OfferId = c.Int(nullable: false),
                        AdvertiserId = c.Int(),
                        OfferName = c.String(),
                        DefaultPriceFormatName = c.String(),
                        CurrencyAbbr = c.String(),
                    })
                .PrimaryKey(t => t.OfferId)
                .ForeignKey("cake.Advertiser", t => t.AdvertiserId)
                .Index(t => t.AdvertiserId);
            
            CreateTable(
                "cake.OfferDailySummary",
                c => new
                    {
                        OfferId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Views = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Paid = c.Int(nullable: false),
                        Sellable = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.OfferId, t.Date });
            
        }
        
        public override void Down()
        {
            DropIndex("cake.Offer", new[] { "AdvertiserId" });
            DropIndex("dbo.OfferInfo", new[] { "OfferId" });
            DropForeignKey("cake.Offer", "AdvertiserId", "cake.Advertiser");
            DropForeignKey("dbo.OfferInfo", "OfferId", "cake.Offer");
            DropTable("cake.OfferDailySummary");
            DropTable("cake.Offer");
            DropTable("cake.Advertiser");
            DropTable("dbo.OfferInfo");
        }
    }
}
