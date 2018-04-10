namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBM : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbm.CreativeDailySummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        CreativeID = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.CreativeID })
                .ForeignKey("dbm.Creative", t => t.CreativeID, cascadeDelete: true)
                .Index(t => t.CreativeID);
            
            CreateTable(
                "dbm.Creative",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        Name = c.String(),
                        InsertionOrderID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbm.InsertionOrder", t => t.InsertionOrderID, cascadeDelete: true)
                .Index(t => t.InsertionOrderID);
            
            CreateTable(
                "dbm.InsertionOrder",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        Name = c.String(),
                        Bucket = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbm.CreativeDailySummary", "CreativeID", "dbm.Creative");
            DropForeignKey("dbm.Creative", "InsertionOrderID", "dbm.InsertionOrder");
            DropIndex("dbm.Creative", new[] { "InsertionOrderID" });
            DropIndex("dbm.CreativeDailySummary", new[] { "CreativeID" });
            DropTable("dbm.InsertionOrder");
            DropTable("dbm.Creative");
            DropTable("dbm.CreativeDailySummary");
        }
    }
}
