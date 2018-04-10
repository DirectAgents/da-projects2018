namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_RmAcctPayInv : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ab.AcctInvoiceBit", "AcctInvoiceId", "ab.AcctInvoice");
            DropForeignKey("ab.AcctPaymentBit", "AcctInvoiceBitId", "ab.AcctInvoiceBit");
            DropForeignKey("ab.AcctPaymentBit", "AcctPaymentId", "ab.AcctPayment");
            DropForeignKey("ab.AcctPayment", "AcctId", "ab.ClientAccount");
            DropForeignKey("ab.AcctInvoice", "AcctId", "ab.ClientAccount");
            DropIndex("ab.AcctInvoice", new[] { "AcctId" });
            DropIndex("ab.AcctInvoiceBit", new[] { "AcctInvoiceId" });
            DropIndex("ab.AcctPaymentBit", new[] { "AcctPaymentId" });
            DropIndex("ab.AcctPaymentBit", new[] { "AcctInvoiceBitId" });
            DropIndex("ab.AcctPayment", new[] { "AcctId" });
            DropTable("ab.AcctInvoice");
            DropTable("ab.AcctInvoiceBit");
            DropTable("ab.AcctPaymentBit");
            DropTable("ab.AcctPayment");
        }
        
        public override void Down()
        {
            CreateTable(
                "ab.AcctPayment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ab.AcctPaymentBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctPaymentId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                        AcctInvoiceBitId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ab.AcctInvoiceBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctInvoiceId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ab.AcctInvoice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("ab.AcctPayment", "AcctId");
            CreateIndex("ab.AcctPaymentBit", "AcctInvoiceBitId");
            CreateIndex("ab.AcctPaymentBit", "AcctPaymentId");
            CreateIndex("ab.AcctInvoiceBit", "AcctInvoiceId");
            CreateIndex("ab.AcctInvoice", "AcctId");
            AddForeignKey("ab.AcctInvoice", "AcctId", "ab.ClientAccount", "Id", cascadeDelete: true);
            AddForeignKey("ab.AcctPayment", "AcctId", "ab.ClientAccount", "Id", cascadeDelete: true);
            AddForeignKey("ab.AcctPaymentBit", "AcctPaymentId", "ab.AcctPayment", "Id", cascadeDelete: true);
            AddForeignKey("ab.AcctPaymentBit", "AcctInvoiceBitId", "ab.AcctInvoiceBit", "Id");
            AddForeignKey("ab.AcctInvoiceBit", "AcctInvoiceId", "ab.AcctInvoice", "Id", cascadeDelete: true);
        }
    }
}
