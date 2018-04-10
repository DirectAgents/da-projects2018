namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Account_Disabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Account", "Disabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("td.Account", "Disabled");
        }
    }
}
