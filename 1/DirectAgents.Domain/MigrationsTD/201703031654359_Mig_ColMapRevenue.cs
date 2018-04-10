namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ColMapRevenue : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.PlatColMapping", "PostClickRev", c => c.String());
            AddColumn("td.PlatColMapping", "PostViewRev", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.PlatColMapping", "PostViewRev");
            DropColumn("td.PlatColMapping", "PostClickRev");
        }
    }
}
