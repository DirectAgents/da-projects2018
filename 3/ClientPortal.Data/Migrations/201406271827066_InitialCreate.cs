namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbm.InsertionOrder",
                c => new
                    {
                        InsertionOrderID = c.Int(nullable: false),
                        InsertionOrderName = c.String(),
                    })
                .PrimaryKey(t => t.InsertionOrderID);
            
            CreateTable(
                "dbm.DailySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        InsertionOrderID = c.Int(nullable: false),
                        AdvertiserCurrency = c.String(),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.Date, t.InsertionOrderID })
                .ForeignKey("dbm.InsertionOrder", t => t.InsertionOrderID, cascadeDelete: true)
                .Index(t => t.InsertionOrderID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbm.DailySummary", new[] { "InsertionOrderID" });
            DropForeignKey("dbm.DailySummary", "InsertionOrderID", "dbm.InsertionOrder");
            DropTable("dbm.DailySummary");
            DropTable("dbm.InsertionOrder");
        }
    }
}
