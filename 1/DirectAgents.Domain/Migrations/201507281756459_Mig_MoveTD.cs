namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_MoveTD : DbMigration
    {
        // Note: Entities moved to DATDContext

        public override void Up()
        {
            //DropForeignKey("adr.AdvertisableStat", "AdvertisableId", "adr.Advertisable");
            //DropIndex("adr.AdvertisableStat", new[] { "AdvertisableId" });
            //DropTable("adr.Advertisable");
            //DropTable("adr.AdvertisableStat");
        }
        
        public override void Down()
        {
            //CreateTable(
            //    "adr.AdvertisableStat",
            //    c => new
            //        {
            //            Date = c.DateTime(nullable: false),
            //            AdvertisableId = c.Int(nullable: false),
            //            Impressions = c.Int(nullable: false),
            //            Clicks = c.Int(nullable: false),
            //            CTC = c.Int(nullable: false),
            //            VTC = c.Int(nullable: false),
            //            Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
            //            Prospects = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => new { t.Date, t.AdvertisableId });
            
            //CreateTable(
            //    "adr.Advertisable",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Name = c.String(),
            //            Eid = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateIndex("adr.AdvertisableStat", "AdvertisableId");
            //AddForeignKey("adr.AdvertisableStat", "AdvertisableId", "adr.Advertisable", "Id", cascadeDelete: true);
        }
    }
}
