namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_CampSumFKs : DbMigration
    {
        public override void Up()
        {
            CreateIndex("cake.Camp", "AffiliateId");
            CreateIndex("cake.Camp", "OfferId");
            CreateIndex("cake.CampSum", "CampId");
            AddForeignKey("cake.Camp", "AffiliateId", "cake.Affiliate", "AffiliateId", cascadeDelete: true);
            AddForeignKey("cake.Camp", "OfferId", "cake.Offer", "OfferId", cascadeDelete: true);
            AddForeignKey("cake.CampSum", "CampId", "cake.Camp", "CampaignId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("cake.CampSum", "CampId", "cake.Camp");
            DropForeignKey("cake.Camp", "OfferId", "cake.Offer");
            DropForeignKey("cake.Camp", "AffiliateId", "cake.Affiliate");
            DropIndex("cake.CampSum", new[] { "CampId" });
            DropIndex("cake.Camp", new[] { "OfferId" });
            DropIndex("cake.Camp", new[] { "AffiliateId" });
        }
    }
}
