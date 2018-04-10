namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_TDShowConversions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradingDeskAccount", "ShowConversions", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TradingDeskAccount", "ShowConversions");
        }
    }
}
