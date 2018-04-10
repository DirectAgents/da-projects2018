namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_RmIdentity_TDA : DbMigration
    {
        public override void Up()
        {
            this.ChangeIdentity(IdentityChange.SwitchIdentityOff, "dbo.TradingDeskAccount", "TradingDeskAccountId")
                .WithDependentColumn("adr.AdRollProfile", "TradingDeskAccountId")
                .WithDependentColumn("dbm.InsertionOrder", "TradingDeskAccountId");
            //DropForeignKey("adr.AdRollProfile", "TradingDeskAccountId", "dbo.TradingDeskAccount");
            //DropForeignKey("dbm.InsertionOrder", "TradingDeskAccountId", "dbo.TradingDeskAccount");
            //DropPrimaryKey("dbo.TradingDeskAccount");
            //AlterColumn("dbo.TradingDeskAccount", "TradingDeskAccountId", c => c.Int(nullable: false));
            //AddPrimaryKey("dbo.TradingDeskAccount", "TradingDeskAccountId");
            //AddForeignKey("adr.AdRollProfile", "TradingDeskAccountId", "dbo.TradingDeskAccount", "TradingDeskAccountId");
            //AddForeignKey("dbm.InsertionOrder", "TradingDeskAccountId", "dbo.TradingDeskAccount", "TradingDeskAccountId");
        }
        
        public override void Down()
        {
            this.ChangeIdentity(IdentityChange.SwitchIdentityOn, "dbo.TradingDeskAccount", "TradingDeskAccountId")
                .WithDependentColumn("adr.AdRollProfile", "TradingDeskAccountId")
                .WithDependentColumn("dbm.InsertionOrder", "TradingDeskAccountId");
            //DropForeignKey("dbm.InsertionOrder", "TradingDeskAccountId", "dbo.TradingDeskAccount");
            //DropForeignKey("adr.AdRollProfile", "TradingDeskAccountId", "dbo.TradingDeskAccount");
            //DropPrimaryKey("dbo.TradingDeskAccount");
            //AlterColumn("dbo.TradingDeskAccount", "TradingDeskAccountId", c => c.Int(nullable: false, identity: true));
            //AddPrimaryKey("dbo.TradingDeskAccount", "TradingDeskAccountId");
            //AddForeignKey("dbm.InsertionOrder", "TradingDeskAccountId", "dbo.TradingDeskAccount", "TradingDeskAccountId");
            //AddForeignKey("adr.AdRollProfile", "TradingDeskAccountId", "dbo.TradingDeskAccount", "TradingDeskAccountId");
        }
    }
}
