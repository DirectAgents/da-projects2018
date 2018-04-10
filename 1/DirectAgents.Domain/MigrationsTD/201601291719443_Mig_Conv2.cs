namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Conv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("td.Conv", "AccountId", c => c.Int(nullable: false));
            CreateIndex("td.Conv", "AccountId");
            AddForeignKey("td.Conv", "AccountId", "td.Account", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("td.Conv", "AccountId", "td.Account");
            DropIndex("td.Conv", new[] { "AccountId" });
            DropColumn("td.Conv", "AccountId");
        }
    }
}
