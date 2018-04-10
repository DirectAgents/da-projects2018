namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_TDad_Url : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Ad", "Url", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.Ad", "Url");
        }
    }
}
