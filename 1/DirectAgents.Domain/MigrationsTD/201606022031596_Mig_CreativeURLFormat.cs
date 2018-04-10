namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_CreativeURLFormat : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Account", "CreativeURLFormat", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.Account", "CreativeURLFormat");
        }
    }
}
