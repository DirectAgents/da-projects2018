namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_FixedMetric : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TradingDeskAccount", "FixedMetricName", c => c.String());
            AddColumn("dbo.TradingDeskAccount", "FixedMetricValue", c => c.Decimal(precision: 18, scale: 6));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TradingDeskAccount", "FixedMetricValue");
            DropColumn("dbo.TradingDeskAccount", "FixedMetricName");
        }
    }
}
