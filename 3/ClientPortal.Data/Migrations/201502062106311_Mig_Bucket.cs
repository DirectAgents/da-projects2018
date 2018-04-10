namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Bucket : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbm.InsertionOrder", "Bucket", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbm.InsertionOrder", "Bucket");
        }
    }
}
