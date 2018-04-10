namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_DBMConversionFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbm.Conversion", "EventSubType", c => c.String());
            AddColumn("dbm.Conversion", "UserID", c => c.String());
            AddColumn("dbm.Conversion", "AdPosition", c => c.Int());
            AddColumn("dbm.Conversion", "Country", c => c.String());
            AddColumn("dbm.Conversion", "DMACode", c => c.Int());
            AddColumn("dbm.Conversion", "PostalCode", c => c.String());
            AddColumn("dbm.Conversion", "GeoRegionID", c => c.Int());
            AddColumn("dbm.Conversion", "CityID", c => c.Int());
            AddColumn("dbm.Conversion", "OSID", c => c.Int());
            AddColumn("dbm.Conversion", "BrowserID", c => c.Int());
            AddColumn("dbm.Conversion", "BrowserTimezoneOffsetMinutes", c => c.Int());
            AddColumn("dbm.Conversion", "NetSpeed", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbm.Conversion", "NetSpeed");
            DropColumn("dbm.Conversion", "BrowserTimezoneOffsetMinutes");
            DropColumn("dbm.Conversion", "BrowserID");
            DropColumn("dbm.Conversion", "OSID");
            DropColumn("dbm.Conversion", "CityID");
            DropColumn("dbm.Conversion", "GeoRegionID");
            DropColumn("dbm.Conversion", "PostalCode");
            DropColumn("dbm.Conversion", "DMACode");
            DropColumn("dbm.Conversion", "Country");
            DropColumn("dbm.Conversion", "AdPosition");
            DropColumn("dbm.Conversion", "UserID");
            DropColumn("dbm.Conversion", "EventSubType");
        }
    }
}
