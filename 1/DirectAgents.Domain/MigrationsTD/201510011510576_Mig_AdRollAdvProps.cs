namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AdRollAdvProps : DbMigration
    {
        public override void Up()
        {
            AddColumn("adr.Advertisable", "Active", c => c.Boolean(nullable: false));
            AddColumn("adr.Advertisable", "Status", c => c.String());
            AddColumn("adr.Advertisable", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("adr.Advertisable", "UpdatedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("adr.Advertisable", "UpdatedDate");
            DropColumn("adr.Advertisable", "CreatedDate");
            DropColumn("adr.Advertisable", "Status");
            DropColumn("adr.Advertisable", "Active");
        }
    }
}
