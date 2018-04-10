namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ModDbmDailySummaries : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbm.DailySummary", "Revenue", c => c.Decimal(nullable: false, precision: 18, scale: 6));
            AlterColumn("dbm.CreativeDailySummary", "Revenue", c => c.Decimal(nullable: false, precision: 18, scale: 6));
            DropColumn("dbm.DailySummary", "AdvertiserCurrency");
            DropColumn("dbm.CreativeDailySummary", "AdvertiserCurrency");
        }
        
        public override void Down()
        {
            AddColumn("dbm.CreativeDailySummary", "AdvertiserCurrency", c => c.String());
            AddColumn("dbm.DailySummary", "AdvertiserCurrency", c => c.String());
            AlterColumn("dbm.CreativeDailySummary", "Revenue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbm.DailySummary", "Revenue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
