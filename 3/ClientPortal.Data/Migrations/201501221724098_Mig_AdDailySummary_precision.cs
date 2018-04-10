namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdDailySummary_precision : DbMigration
    {
        public override void Up()
        {
            AlterColumn("adr.AdDailySummary", "Spend", c => c.Decimal(nullable: false, precision: 18, scale: 6));
        }
        
        public override void Down()
        {
            AlterColumn("adr.AdDailySummary", "Spend", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
