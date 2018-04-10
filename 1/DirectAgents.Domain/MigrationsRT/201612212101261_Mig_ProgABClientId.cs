namespace DirectAgents.Domain.MigrationsRT
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ProgABClientId : DbMigration
    {
        public override void Up()
        {
            AddColumn("ext.ProgClient", "ABClientId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("ext.ProgClient", "ABClientId");
        }
    }
}
