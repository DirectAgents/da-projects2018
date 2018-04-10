namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DailyLocationStat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbm.City",
                c => new
                    {
                        CityID = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CityID);
            
            CreateTable(
                "dbm.DailyLocationStat",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        InsertionOrderID = c.Int(nullable: false),
                        CityID = c.Int(nullable: false),
                        RegionID = c.Int(nullable: false),
                        DMACode = c.Int(nullable: false),
                        CountryAbbrev = c.String(nullable: false, maxLength: 8),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 18, scale: 6),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.InsertionOrderID, t.CityID, t.RegionID, t.DMACode, t.CountryAbbrev })
                .ForeignKey("dbm.City", t => t.CityID, cascadeDelete: true)
                .ForeignKey("dbm.DMA", t => t.DMACode, cascadeDelete: true)
                .ForeignKey("dbm.InsertionOrder", t => t.InsertionOrderID, cascadeDelete: true)
                .ForeignKey("dbm.Region", t => t.RegionID, cascadeDelete: true)
                .Index(t => t.InsertionOrderID)
                .Index(t => t.CityID)
                .Index(t => t.RegionID)
                .Index(t => t.DMACode);
            
            CreateTable(
                "dbm.DMA",
                c => new
                    {
                        DMACode = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.DMACode);
            
            CreateTable(
                "dbm.Region",
                c => new
                    {
                        RegionID = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.RegionID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbm.DailyLocationStat", "RegionID", "dbm.Region");
            DropForeignKey("dbm.DailyLocationStat", "InsertionOrderID", "dbm.InsertionOrder");
            DropForeignKey("dbm.DailyLocationStat", "DMACode", "dbm.DMA");
            DropForeignKey("dbm.DailyLocationStat", "CityID", "dbm.City");
            DropIndex("dbm.DailyLocationStat", new[] { "DMACode" });
            DropIndex("dbm.DailyLocationStat", new[] { "RegionID" });
            DropIndex("dbm.DailyLocationStat", new[] { "CityID" });
            DropIndex("dbm.DailyLocationStat", new[] { "InsertionOrderID" });
            DropTable("dbm.Region");
            DropTable("dbm.DMA");
            DropTable("dbm.DailyLocationStat");
            DropTable("dbm.City");
        }
    }
}
