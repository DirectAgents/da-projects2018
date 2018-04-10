namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_BodyHeadlineMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Ad", "Body", c => c.String());
            AddColumn("td.Ad", "Headline", c => c.String());
            AddColumn("td.Ad", "Message", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("td.Ad", "Message");
            DropColumn("td.Ad", "Headline");
            DropColumn("td.Ad", "Body");
        }
    }
}
