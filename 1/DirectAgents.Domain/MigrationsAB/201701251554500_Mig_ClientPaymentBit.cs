namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ClientPaymentBit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.ClientPaymentBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientPaymentId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientPayment", t => t.ClientPaymentId, cascadeDelete: true)
                .Index(t => t.ClientPaymentId);
            
            DropColumn("ab.ClientPayment", "Value");
        }
        
        public override void Down()
        {
            AddColumn("ab.ClientPayment", "Value", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            DropForeignKey("ab.ClientPaymentBit", "ClientPaymentId", "ab.ClientPayment");
            DropIndex("ab.ClientPaymentBit", new[] { "ClientPaymentId" });
            DropTable("ab.ClientPaymentBit");
        }
    }
}
