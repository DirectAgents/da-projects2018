namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBMDailySummary : DbMigration
    {
        // The entity DailySummary was renamed to DBMDailySummary, but is mapped to the same database table
        // ("DailySummary" under the "dbm" schema)
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }
    }
}
