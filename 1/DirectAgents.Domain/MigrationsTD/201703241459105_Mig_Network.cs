namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Network : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.Network",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("td.Account", "NetworkId", c => c.Int());
            CreateIndex("td.Account", "NetworkId");
            AddForeignKey("td.Account", "NetworkId", "td.Network", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("td.Account", "NetworkId", "td.Network");
            DropIndex("td.Account", new[] { "NetworkId" });
            DropColumn("td.Account", "NetworkId");
            DropTable("td.Network");
        }
    }
}
