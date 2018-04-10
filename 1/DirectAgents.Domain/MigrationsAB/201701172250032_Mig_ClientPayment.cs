namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ClientPayment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.ClientPayment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Client", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.ClientPayment", "ClientId", "ab.Client");
            DropIndex("ab.ClientPayment", new[] { "ClientId" });
            DropTable("ab.ClientPayment");
        }
    }
}
