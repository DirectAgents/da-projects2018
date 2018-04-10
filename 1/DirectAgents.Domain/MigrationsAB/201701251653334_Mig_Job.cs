namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Job : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.Job",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Client", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.Job", "ClientId", "ab.Client");
            DropIndex("ab.Job", new[] { "ClientId" });
            DropTable("ab.Job");
        }
    }
}
