namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBMConversion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbm.Conversion",
                c => new
                    {
                        AuctionID = c.String(nullable: false, maxLength: 128),
                        EventTime = c.DateTime(nullable: false, storeType: "datetime2"),
                        ViewTime = c.DateTime(storeType: "datetime2"),
                        RequestTime = c.DateTime(storeType: "datetime2"),
                        InsertionOrderID = c.Int(),
                        LineItemID = c.Int(),
                        CreativeID = c.Int(),
                    })
                .PrimaryKey(t => new { t.AuctionID, t.EventTime });
            
        }
        
        public override void Down()
        {
            DropTable("dbm.Conversion");
        }
    }
}
