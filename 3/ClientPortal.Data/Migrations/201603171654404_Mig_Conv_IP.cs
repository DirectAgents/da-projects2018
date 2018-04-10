namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Conv_IP : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbm.Conversion", "IP", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbm.Conversion", "IP");
        }
    }
}
