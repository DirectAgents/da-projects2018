namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBM_CDS_AddIOID : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbm.Creative", "InsertionOrderID", "dbm.InsertionOrder");
            DropIndex("dbm.Creative", new[] { "InsertionOrderID" });
            DropPrimaryKey("dbm.CreativeDailySummary");
            AddColumn("dbm.CreativeDailySummary", "InsertionOrderID", c => c.Int(nullable: false));
            AddPrimaryKey("dbm.CreativeDailySummary", new[] { "Date", "InsertionOrderID", "CreativeID" });
            CreateIndex("dbm.CreativeDailySummary", "InsertionOrderID");
            AddForeignKey("dbm.CreativeDailySummary", "InsertionOrderID", "dbm.InsertionOrder", "ID", cascadeDelete: true);
            DropColumn("dbm.Creative", "InsertionOrderID");
        }
        
        public override void Down()
        {
            AddColumn("dbm.Creative", "InsertionOrderID", c => c.Int(nullable: false));
            DropForeignKey("dbm.CreativeDailySummary", "InsertionOrderID", "dbm.InsertionOrder");
            DropIndex("dbm.CreativeDailySummary", new[] { "InsertionOrderID" });
            DropPrimaryKey("dbm.CreativeDailySummary");
            DropColumn("dbm.CreativeDailySummary", "InsertionOrderID");
            AddPrimaryKey("dbm.CreativeDailySummary", new[] { "Date", "CreativeID" });
            CreateIndex("dbm.Creative", "InsertionOrderID");
            AddForeignKey("dbm.Creative", "InsertionOrderID", "dbm.InsertionOrder", "ID", cascadeDelete: true);
        }
    }
}
