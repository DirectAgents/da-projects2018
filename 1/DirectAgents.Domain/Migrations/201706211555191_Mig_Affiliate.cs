namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Affiliate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.Affiliate",
                c => new
                    {
                        AffiliateId = c.Int(nullable: false),
                        AffiliateName = c.String(),
                    })
                .PrimaryKey(t => t.AffiliateId);
            
        }
        
        public override void Down()
        {
            DropTable("cake.Affiliate");
        }
    }
}
