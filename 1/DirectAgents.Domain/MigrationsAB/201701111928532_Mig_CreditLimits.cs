namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_CreditLimits : DbMigration
    {
        public override void Up()
        {
            AddColumn("ab.ClientAccount", "ExtCredit", c => c.Decimal(nullable: false, precision: 14, scale: 2));
            AddColumn("ab.ClientAccount", "IntCredit", c => c.Decimal(nullable: false, precision: 14, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("ab.ClientAccount", "IntCredit");
            DropColumn("ab.ClientAccount", "ExtCredit");
        }
    }
}
