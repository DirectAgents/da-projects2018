namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_EventConversion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.EventConversion",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ConvDate = c.DateTime(nullable: false),
                        ClickDate = c.DateTime(nullable: false),
                        EventId = c.Int(nullable: false),
                        AffiliateId = c.Int(nullable: false),
                        OfferId = c.Int(nullable: false),
                        SubId1 = c.String(),
                        SubId2 = c.String(),
                        SubId3 = c.String(),
                        SubId4 = c.String(),
                        SubId5 = c.String(),
                        PriceFormatId = c.Int(nullable: false),
                        Paid = c.Decimal(nullable: false, precision: 19, scale: 4),
                        Received = c.Decimal(nullable: false, precision: 19, scale: 4),
                        PaidCurrId = c.Int(nullable: false),
                        ReceivedCurrId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("cake.Affiliate", t => t.AffiliateId, cascadeDelete: true)
                .ForeignKey("cake.Event", t => t.EventId, cascadeDelete: true)
                .ForeignKey("cake.Offer", t => t.OfferId, cascadeDelete: true)
                .ForeignKey("dbo.Currency", t => t.PaidCurrId)
                .ForeignKey("cake.PriceFormat", t => t.PriceFormatId, cascadeDelete: true)
                .ForeignKey("dbo.Currency", t => t.ReceivedCurrId)
                .Index(t => t.EventId)
                .Index(t => t.AffiliateId)
                .Index(t => t.OfferId)
                .Index(t => t.PriceFormatId)
                .Index(t => t.PaidCurrId)
                .Index(t => t.ReceivedCurrId);
            
            CreateTable(
                "cake.Event",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "cake.PriceFormat",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("cake.EventConversion", "ReceivedCurrId", "dbo.Currency");
            DropForeignKey("cake.EventConversion", "PriceFormatId", "cake.PriceFormat");
            DropForeignKey("cake.EventConversion", "PaidCurrId", "dbo.Currency");
            DropForeignKey("cake.EventConversion", "OfferId", "cake.Offer");
            DropForeignKey("cake.EventConversion", "EventId", "cake.Event");
            DropForeignKey("cake.EventConversion", "AffiliateId", "cake.Affiliate");
            DropIndex("cake.EventConversion", new[] { "ReceivedCurrId" });
            DropIndex("cake.EventConversion", new[] { "PaidCurrId" });
            DropIndex("cake.EventConversion", new[] { "PriceFormatId" });
            DropIndex("cake.EventConversion", new[] { "OfferId" });
            DropIndex("cake.EventConversion", new[] { "AffiliateId" });
            DropIndex("cake.EventConversion", new[] { "EventId" });
            DropTable("cake.PriceFormat");
            DropTable("cake.Event");
            DropTable("cake.EventConversion");
        }
    }
}
