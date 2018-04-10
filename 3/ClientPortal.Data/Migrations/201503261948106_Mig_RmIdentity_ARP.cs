namespace ClientPortal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_RmIdentity_ARP : DbMigration
    {
        public override void Up()
        {
            this.ChangeIdentity(IdentityChange.SwitchIdentityOff, "adr.AdRollProfile", "Id")
                .WithDependentColumn("adr.AdRollAd", "AdRollProfileId");
        }
        
        public override void Down()
        {
            this.ChangeIdentity(IdentityChange.SwitchIdentityOn, "adr.AdRollProfile", "Id")
                .WithDependentColumn("adr.AdRollAd", "AdRollProfileId");
        }
    }
}
