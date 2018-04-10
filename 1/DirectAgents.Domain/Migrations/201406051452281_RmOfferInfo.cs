namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RmOfferInfo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OfferInfo", "OfferId", "cake.Offer");
            DropIndex("dbo.OfferInfo", new[] { "OfferId" });
            DropTable("dbo.OfferInfo");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.OfferInfo",
                c => new
                    {
                        OfferId = c.Int(nullable: false),
                        BudgetIsMonthly = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.OfferId);
            
            CreateIndex("dbo.OfferInfo", "OfferId");
            AddForeignKey("dbo.OfferInfo", "OfferId", "cake.Offer", "OfferId");
        }
    }
}
