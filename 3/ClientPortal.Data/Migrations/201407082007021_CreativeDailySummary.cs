namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreativeDailySummary : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbm.Creative",
                c => new
                    {
                        CreativeID = c.Int(nullable: false),
                        CreativeName = c.String(),
                        InsertionOrderID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CreativeID)
                .ForeignKey("dbm.InsertionOrder", t => t.InsertionOrderID, cascadeDelete: true)
                .Index(t => t.InsertionOrderID);
            
            CreateTable(
                "dbm.CreativeDailySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        CreativeID = c.Int(nullable: false),
                        AdvertiserCurrency = c.String(),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.Date, t.CreativeID })
                .ForeignKey("dbm.Creative", t => t.CreativeID, cascadeDelete: true)
                .Index(t => t.CreativeID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbm.CreativeDailySummary", new[] { "CreativeID" });
            DropIndex("dbm.Creative", new[] { "InsertionOrderID" });
            DropForeignKey("dbm.CreativeDailySummary", "CreativeID", "dbm.Creative");
            DropForeignKey("dbm.Creative", "InsertionOrderID", "dbm.InsertionOrder");
            DropTable("dbm.CreativeDailySummary");
            DropTable("dbm.Creative");
        }
    }
}
