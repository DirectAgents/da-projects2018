namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_CampSum2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("cake.CampSum", "CostPerUnit");
            DropColumn("cake.CampSum", "RevenuePerUnit");
            AddColumn("cake.CampSum", "Units", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            Sql("ALTER TABLE cake.CampSum ADD RevenuePerUnit AS CASE WHEN Units=0 THEN 0 ELSE CAST(Revenue/Units AS decimal(21, 6)) END");
            Sql("ALTER TABLE cake.CampSum ADD CostPerUnit AS CASE WHEN Units=0 THEN 0 ELSE CAST(Cost/Units AS decimal(21, 6)) END");
            AddColumn("cake.CampSum", "PriceFormat", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("cake.CampSum", "PriceFormat");
            DropColumn("cake.CampSum", "CostPerUnit");
            DropColumn("cake.CampSum", "RevenuePerUnit");
            DropColumn("cake.CampSum", "Units");
            Sql("ALTER TABLE cake.CampSum ADD RevenuePerUnit AS CASE WHEN Paid=0 THEN 0 ELSE CAST(Revenue/Paid AS decimal(21, 6)) END");
            Sql("ALTER TABLE cake.CampSum ADD CostPerUnit AS CASE WHEN Paid=0 THEN 0 ELSE CAST(Cost/Paid AS decimal(21, 6)) END");
        }
    }
}
