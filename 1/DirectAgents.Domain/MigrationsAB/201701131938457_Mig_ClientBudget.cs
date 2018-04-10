namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ClientBudget : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.ClientBudget",
                c => new
                    {
                        ClientId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => new { t.ClientId, t.Date })
                .ForeignKey("ab.Client", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
            AddColumn("ab.Client", "ExtCredit", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            AddColumn("ab.Client", "IntCredit", c => c.Decimal(nullable: false, precision: 14, scale: 2));
        }
        
        public override void Down()
        {
            DropForeignKey("ab.ClientBudget", "ClientId", "ab.Client");
            DropIndex("ab.ClientBudget", new[] { "ClientId" });
            DropColumn("ab.Client", "IntCredit");
            DropColumn("ab.Client", "ExtCredit");
            DropTable("ab.ClientBudget");
        }
    }
}
