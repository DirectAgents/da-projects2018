namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_AcctPayInvCamp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.AcctPayment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.AcctId, cascadeDelete: true)
                .Index(t => t.AcctId);
            
            CreateTable(
                "ab.AcctPaymentBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctPaymentId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                        AcctInvoiceBitId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.AcctInvoiceBit", t => t.AcctInvoiceBitId)
                .ForeignKey("ab.AcctPayment", t => t.AcctPaymentId, cascadeDelete: true)
                .Index(t => t.AcctPaymentId)
                .Index(t => t.AcctInvoiceBitId);
            
            CreateTable(
                "ab.AcctInvoiceBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctInvoiceId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                        AcctSpendBitId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.AcctInvoice", t => t.AcctInvoiceId, cascadeDelete: true)
                .ForeignKey("ab.AcctSpendBit", t => t.AcctSpendBitId)
                .Index(t => t.AcctInvoiceId)
                .Index(t => t.AcctSpendBitId);
            
            CreateTable(
                "ab.AcctInvoice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.AcctId, cascadeDelete: true)
                .Index(t => t.AcctId);
            
            CreateTable(
                "ab.AcctSpendBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        AcctSpendBucketId = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Desc = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Period", t => t.PeriodId, cascadeDelete: true)
                .ForeignKey("ab.AcctSpendBucket", t => t.AcctSpendBucketId, cascadeDelete: true)
                .Index(t => t.PeriodId)
                .Index(t => t.AcctSpendBucketId);
            
            CreateTable(
                "ab.Period",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ab.AcctSpendBucket",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AcctId = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.AcctId, cascadeDelete: true)
                .Index(t => t.AcctId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.AcctPayment", "AcctId", "ab.ClientAccount");
            DropForeignKey("ab.AcctPaymentBit", "AcctPaymentId", "ab.AcctPayment");
            DropForeignKey("ab.AcctSpendBucket", "AcctId", "ab.ClientAccount");
            DropForeignKey("ab.AcctSpendBit", "AcctSpendBucketId", "ab.AcctSpendBucket");
            DropForeignKey("ab.AcctSpendBit", "PeriodId", "ab.Period");
            DropForeignKey("ab.AcctInvoiceBit", "AcctSpendBitId", "ab.AcctSpendBit");
            DropForeignKey("ab.AcctPaymentBit", "AcctInvoiceBitId", "ab.AcctInvoiceBit");
            DropForeignKey("ab.AcctInvoice", "AcctId", "ab.ClientAccount");
            DropForeignKey("ab.AcctInvoiceBit", "AcctInvoiceId", "ab.AcctInvoice");
            DropIndex("ab.AcctSpendBucket", new[] { "AcctId" });
            DropIndex("ab.AcctSpendBit", new[] { "AcctSpendBucketId" });
            DropIndex("ab.AcctSpendBit", new[] { "PeriodId" });
            DropIndex("ab.AcctInvoice", new[] { "AcctId" });
            DropIndex("ab.AcctInvoiceBit", new[] { "AcctSpendBitId" });
            DropIndex("ab.AcctInvoiceBit", new[] { "AcctInvoiceId" });
            DropIndex("ab.AcctPaymentBit", new[] { "AcctInvoiceBitId" });
            DropIndex("ab.AcctPaymentBit", new[] { "AcctPaymentId" });
            DropIndex("ab.AcctPayment", new[] { "AcctId" });
            DropTable("ab.AcctSpendBucket");
            DropTable("ab.Period");
            DropTable("ab.AcctSpendBit");
            DropTable("ab.AcctInvoice");
            DropTable("ab.AcctInvoiceBit");
            DropTable("ab.AcctPaymentBit");
            DropTable("ab.AcctPayment");
        }
    }
}
