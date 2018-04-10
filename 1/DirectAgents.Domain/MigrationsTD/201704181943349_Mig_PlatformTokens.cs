namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_PlatformTokens : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Platform", "Tokens", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.Platform", "Tokens");
        }
    }
}
