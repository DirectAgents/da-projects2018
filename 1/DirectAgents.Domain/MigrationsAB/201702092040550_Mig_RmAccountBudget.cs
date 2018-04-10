namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_RmAccountBudget : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ab.AccountBudget", "ClientAccountId", "ab.ClientAccount");
            DropIndex("ab.AccountBudget", new[] { "ClientAccountId" });
            DropTable("ab.AccountBudget");
        }
        
        public override void Down()
        {
            CreateTable(
                "ab.AccountBudget",
                c => new
                    {
                        ClientAccountId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => new { t.ClientAccountId, t.Date });
            
            CreateIndex("ab.AccountBudget", "ClientAccountId");
            AddForeignKey("ab.AccountBudget", "ClientAccountId", "ab.ClientAccount", "Id", cascadeDelete: true);
        }
    }
}
