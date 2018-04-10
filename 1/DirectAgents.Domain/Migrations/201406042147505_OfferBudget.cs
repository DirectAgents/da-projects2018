namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OfferBudget : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OfferBudget",
                c => new
                    {
                        OfferBudgetId = c.Int(nullable: false, identity: true),
                        OfferId = c.Int(nullable: false),
                        Budget = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OfferBudgetId)
                .ForeignKey("cake.Offer", t => t.OfferId, cascadeDelete: true)
                .Index(t => t.OfferId);
            
            DropColumn("dbo.OfferInfo", "Budget");
            DropColumn("dbo.OfferInfo", "BudgetStart");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OfferInfo", "BudgetStart", c => c.DateTime());
            AddColumn("dbo.OfferInfo", "Budget", c => c.Decimal(precision: 18, scale: 2));
            DropIndex("dbo.OfferBudget", new[] { "OfferId" });
            DropForeignKey("dbo.OfferBudget", "OfferId", "cake.Offer");
            DropTable("dbo.OfferBudget");
        }
    }
}
