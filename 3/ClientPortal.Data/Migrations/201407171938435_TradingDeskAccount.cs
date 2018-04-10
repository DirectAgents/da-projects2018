namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TradingDeskAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TradingDeskAccount",
                c => new
                    {
                        TradingDeskAccountId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.TradingDeskAccountId);
            
            AddColumn("dbm.InsertionOrder", "TradingDeskAccountId", c => c.Int());
            AddForeignKey("dbm.InsertionOrder", "TradingDeskAccountId", "dbo.TradingDeskAccount", "TradingDeskAccountId");
            CreateIndex("dbm.InsertionOrder", "TradingDeskAccountId");
        }
        
        public override void Down()
        {
            DropIndex("dbm.InsertionOrder", new[] { "TradingDeskAccountId" });
            DropForeignKey("dbm.InsertionOrder", "TradingDeskAccountId", "dbo.TradingDeskAccount");
            DropColumn("dbm.InsertionOrder", "TradingDeskAccountId");
            DropTable("dbo.TradingDeskAccount");
        }
    }
}
