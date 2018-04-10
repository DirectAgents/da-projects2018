namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Payment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.Payment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Client", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
            CreateTable(
                "ab.PaymentBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentId = c.Int(nullable: false),
                        AcctId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.AcctId)
                .ForeignKey("ab.Payment", t => t.PaymentId, cascadeDelete: true)
                .Index(t => t.PaymentId)
                .Index(t => t.AcctId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.Payment", "ClientId", "ab.Client");
            DropForeignKey("ab.PaymentBit", "PaymentId", "ab.Payment");
            DropForeignKey("ab.PaymentBit", "AcctId", "ab.ClientAccount");
            DropIndex("ab.PaymentBit", new[] { "AcctId" });
            DropIndex("ab.PaymentBit", new[] { "PaymentId" });
            DropIndex("ab.Payment", new[] { "ClientId" });
            DropTable("ab.PaymentBit");
            DropTable("ab.Payment");
        }
    }
}
