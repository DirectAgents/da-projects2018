namespace DirectAgents.Domain.MigrationsRT
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ProgCampaign : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ext.ProgCampaign",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ProgClientId = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ext.ProgClient", t => t.ProgClientId, cascadeDelete: true)
                .Index(t => t.ProgClientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ext.ProgCampaign", "ProgClientId", "ext.ProgClient");
            DropIndex("ext.ProgCampaign", new[] { "ProgClientId" });
            DropTable("ext.ProgCampaign");
        }
    }
}
