namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ProtoObjs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.ProtoCampaign",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ClientAccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.ClientAccountId, cascadeDelete: true)
                .Index(t => t.ClientAccountId);
            
            CreateTable(
                "ab.ProtoSpendBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProtoPeriodId = c.Int(nullable: false),
                        ProtoCampaignId = c.Int(nullable: false),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Desc = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ProtoCampaign", t => t.ProtoCampaignId, cascadeDelete: true)
                .ForeignKey("ab.ProtoPeriod", t => t.ProtoPeriodId, cascadeDelete: true)
                .Index(t => t.ProtoPeriodId)
                .Index(t => t.ProtoCampaignId);
            
            CreateTable(
                "ab.ProtoPeriod",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ab.ProtoPaymentBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProtoPaymentId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                        ProtoInvoiceBitId = c.Int(),
                        ClientAccountId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.ClientAccountId)
                .ForeignKey("ab.ProtoInvoiceBit", t => t.ProtoInvoiceBitId)
                .ForeignKey("ab.ProtoPayment", t => t.ProtoPaymentId, cascadeDelete: true)
                .Index(t => t.ProtoPaymentId)
                .Index(t => t.ProtoInvoiceBitId)
                .Index(t => t.ClientAccountId);
            
            CreateTable(
                "ab.ProtoInvoiceBit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProtoInvoiceId = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 14, scale: 2),
                        ProtoSpendBitId = c.Int(),
                        ClientAccountId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.ClientAccount", t => t.ClientAccountId)
                .ForeignKey("ab.ProtoInvoice", t => t.ProtoInvoiceId, cascadeDelete: true)
                .ForeignKey("ab.ProtoSpendBit", t => t.ProtoSpendBitId)
                .Index(t => t.ProtoInvoiceId)
                .Index(t => t.ProtoSpendBitId)
                .Index(t => t.ClientAccountId);
            
            CreateTable(
                "ab.ProtoInvoice",
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
                "ab.ProtoPayment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Client", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
            AddColumn("ab.ClientAccount", "Name", c => c.String());
            DropColumn("ab.ClientAccount", "ExtCredit");
            DropColumn("ab.ClientAccount", "IntCredit");
        }
        
        public override void Down()
        {
            AddColumn("ab.ClientAccount", "IntCredit", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            AddColumn("ab.ClientAccount", "ExtCredit", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            DropForeignKey("ab.ProtoPayment", "ClientId", "ab.Client");
            DropForeignKey("ab.ProtoPaymentBit", "ProtoPaymentId", "ab.ProtoPayment");
            DropForeignKey("ab.ProtoInvoiceBit", "ProtoSpendBitId", "ab.ProtoSpendBit");
            DropForeignKey("ab.ProtoPaymentBit", "ProtoInvoiceBitId", "ab.ProtoInvoiceBit");
            DropForeignKey("ab.ProtoInvoice", "ClientId", "ab.Client");
            DropForeignKey("ab.ProtoInvoiceBit", "ProtoInvoiceId", "ab.ProtoInvoice");
            DropForeignKey("ab.ProtoInvoiceBit", "ClientAccountId", "ab.ClientAccount");
            DropForeignKey("ab.ProtoPaymentBit", "ClientAccountId", "ab.ClientAccount");
            DropForeignKey("ab.ProtoSpendBit", "ProtoPeriodId", "ab.ProtoPeriod");
            DropForeignKey("ab.ProtoSpendBit", "ProtoCampaignId", "ab.ProtoCampaign");
            DropForeignKey("ab.ProtoCampaign", "ClientAccountId", "ab.ClientAccount");
            DropIndex("ab.ProtoPayment", new[] { "ClientId" });
            DropIndex("ab.ProtoInvoice", new[] { "ClientId" });
            DropIndex("ab.ProtoInvoiceBit", new[] { "ClientAccountId" });
            DropIndex("ab.ProtoInvoiceBit", new[] { "ProtoSpendBitId" });
            DropIndex("ab.ProtoInvoiceBit", new[] { "ProtoInvoiceId" });
            DropIndex("ab.ProtoPaymentBit", new[] { "ClientAccountId" });
            DropIndex("ab.ProtoPaymentBit", new[] { "ProtoInvoiceBitId" });
            DropIndex("ab.ProtoPaymentBit", new[] { "ProtoPaymentId" });
            DropIndex("ab.ProtoSpendBit", new[] { "ProtoCampaignId" });
            DropIndex("ab.ProtoSpendBit", new[] { "ProtoPeriodId" });
            DropIndex("ab.ProtoCampaign", new[] { "ClientAccountId" });
            DropColumn("ab.ClientAccount", "Name");
            DropTable("ab.ProtoPayment");
            DropTable("ab.ProtoInvoice");
            DropTable("ab.ProtoInvoiceBit");
            DropTable("ab.ProtoPaymentBit");
            DropTable("ab.ProtoPeriod");
            DropTable("ab.ProtoSpendBit");
            DropTable("ab.ProtoCampaign");
        }
    }
}
