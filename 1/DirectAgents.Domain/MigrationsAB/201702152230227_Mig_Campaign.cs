namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Campaign : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.Campaign",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("ab.AcctSpendBucket", "CampaignId", c => c.Int(nullable: false));
            CreateIndex("ab.AcctSpendBucket", "CampaignId");
            AddForeignKey("ab.AcctSpendBucket", "CampaignId", "ab.Campaign", "Id", cascadeDelete: true);
            DropColumn("ab.AcctSpendBucket", "Name");
        }
        
        public override void Down()
        {
            AddColumn("ab.AcctSpendBucket", "Name", c => c.String());
            DropForeignKey("ab.AcctSpendBucket", "CampaignId", "ab.Campaign");
            DropIndex("ab.AcctSpendBucket", new[] { "CampaignId" });
            DropColumn("ab.AcctSpendBucket", "CampaignId");
            DropTable("ab.Campaign");
        }
    }
}
