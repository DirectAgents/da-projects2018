namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ClientAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.ClientAccount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Client", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
            CreateTable(
                "ab.AccountBudget",
                c => new
                    {
                        ClientAccountId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => new { t.ClientAccountId, t.Date })
                .ForeignKey("ab.ClientAccount", t => t.ClientAccountId, cascadeDelete: true)
                .Index(t => t.ClientAccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.ClientAccount", "ClientId", "ab.Client");
            DropForeignKey("ab.AccountBudget", "ClientAccountId", "ab.ClientAccount");
            DropIndex("ab.AccountBudget", new[] { "ClientAccountId" });
            DropIndex("ab.ClientAccount", new[] { "ClientId" });
            DropTable("ab.AccountBudget");
            DropTable("ab.ClientAccount");
        }
    }
}
