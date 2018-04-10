namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdRollProfile_Eid : DbMigration
    {
        public override void Up()
        {
            AddColumn("adr.AdRollProfile", "Eid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("adr.AdRollProfile", "Eid");
        }
    }
}
