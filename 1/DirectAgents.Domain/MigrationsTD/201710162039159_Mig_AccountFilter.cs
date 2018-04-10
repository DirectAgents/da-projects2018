namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AccountFilter : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Account", "Filter", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.Account", "Filter");
        }
    }
}
