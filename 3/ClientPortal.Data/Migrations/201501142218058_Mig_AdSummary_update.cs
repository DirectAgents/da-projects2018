namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdSummary_update : DbMigration
    {
        public override void Up()
        {
            AddColumn("adr.AdRollAd", "Eid", c => c.String());
            AddColumn("adr.AdRollAd", "Width", c => c.Int(nullable: false));
            AddColumn("adr.AdRollAd", "Height", c => c.Int(nullable: false));
            DropColumn("adr.AdRollAd", "Size");
        }
        
        public override void Down()
        {
            AddColumn("adr.AdRollAd", "Size", c => c.String());
            DropColumn("adr.AdRollAd", "Height");
            DropColumn("adr.AdRollAd", "Width");
            DropColumn("adr.AdRollAd", "Eid");
        }
    }
}
