namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdRollStat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "adr.AdvertisableStat",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        AdvertisableId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        CTC = c.Int(nullable: false),
                        VTC = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                        Prospects = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Date, t.AdvertisableId })
                .ForeignKey("adr.Advertisable", t => t.AdvertisableId, cascadeDelete: true)
                .Index(t => t.AdvertisableId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("adr.AdvertisableStat", "AdvertisableId", "adr.Advertisable");
            DropIndex("adr.AdvertisableStat", new[] { "AdvertisableId" });
            DropTable("adr.AdvertisableStat");
        }
    }
}
