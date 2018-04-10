namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdRollAds : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "adr.AdDailySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        AdId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        CTC = c.Int(nullable: false),
                        VTC = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                        Prospects = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Date, t.AdId })
                .ForeignKey("adr.Ad", t => t.AdId, cascadeDelete: true)
                .Index(t => t.AdId);
            
            CreateTable(
                "adr.Ad",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdvertisableId = c.Int(nullable: false),
                        Name = c.String(),
                        Type = c.String(),
                        CreatedDate = c.DateTime(),
                        Eid = c.String(),
                        Width = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("adr.Advertisable", t => t.AdvertisableId, cascadeDelete: true)
                .Index(t => t.AdvertisableId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("adr.AdDailySummary", "AdId", "adr.Ad");
            DropForeignKey("adr.Ad", "AdvertisableId", "adr.Advertisable");
            DropIndex("adr.Ad", new[] { "AdvertisableId" });
            DropIndex("adr.AdDailySummary", new[] { "AdId" });
            DropTable("adr.Ad");
            DropTable("adr.AdDailySummary");
        }
    }
}
