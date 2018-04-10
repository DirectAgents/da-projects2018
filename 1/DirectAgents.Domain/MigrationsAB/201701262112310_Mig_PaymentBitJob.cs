namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_PaymentBitJob : DbMigration
    {
        public override void Up()
        {
            AddColumn("ab.ClientPaymentBit", "JobId", c => c.Int());
            CreateIndex("ab.ClientPaymentBit", "JobId");
            AddForeignKey("ab.ClientPaymentBit", "JobId", "ab.Job", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("ab.ClientPaymentBit", "JobId", "ab.Job");
            DropIndex("ab.ClientPaymentBit", new[] { "JobId" });
            DropColumn("ab.ClientPaymentBit", "JobId");
        }
    }
}
