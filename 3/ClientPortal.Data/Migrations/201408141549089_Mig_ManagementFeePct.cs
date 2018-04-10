namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ManagementFeePct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradingDeskAccount", "ManagementFeePct", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TradingDeskAccount", "ManagementFeePct");
        }
    }
}
