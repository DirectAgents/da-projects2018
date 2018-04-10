namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_UserListStat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbm.UserListRun",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        InsertionOrderID = c.Int(),
                        Date = c.DateTime(nullable: false),
                        Name = c.String(),
                        Bucket = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbm.InsertionOrder", t => t.InsertionOrderID)
                .Index(t => t.InsertionOrderID);
            
            CreateTable(
                "dbm.UserListStat",
                c => new
                    {
                        UserListRunID = c.Int(nullable: false),
                        UserListID = c.Int(nullable: false),
                        UserListName = c.String(),
                        Cost = c.Single(nullable: false),
                        EligibleCookies = c.Long(nullable: false),
                        MatchRatio = c.Single(nullable: false),
                        PotentialImpressions = c.Long(nullable: false),
                        Uniques = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserListRunID, t.UserListID })
                .ForeignKey("dbm.UserListRun", t => t.UserListRunID, cascadeDelete: true)
                .Index(t => t.UserListRunID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbm.UserListStat", "UserListRunID", "dbm.UserListRun");
            DropForeignKey("dbm.UserListRun", "InsertionOrderID", "dbm.InsertionOrder");
            DropIndex("dbm.UserListStat", new[] { "UserListRunID" });
            DropIndex("dbm.UserListRun", new[] { "InsertionOrderID" });
            DropTable("dbm.UserListStat");
            DropTable("dbm.UserListRun");
        }
    }
}
