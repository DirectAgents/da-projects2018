namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_CampSum : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.CampSum",
                c => new
                    {
                        CampId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        OfferId = c.Int(nullable: false),
                        AffId = c.Int(nullable: false),
                        Views = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        Conversions = c.Decimal(nullable: false, precision: 16, scale: 6),
                        Paid = c.Decimal(nullable: false, precision: 16, scale: 6),
                        Sellable = c.Decimal(nullable: false, precision: 16, scale: 6),
                        Revenue = c.Decimal(nullable: false, precision: 19, scale: 4),
                        Cost = c.Decimal(nullable: false, precision: 19, scale: 4),
                        //RevenuePerUnit = c.Decimal(nullable: false, precision: 19, scale: 4),
                        //CostPerUnit = c.Decimal(nullable: false, precision: 19, scale: 4),
                    })
                .PrimaryKey(t => new { t.CampId, t.Date });
            Sql("ALTER TABLE cake.CampSum ADD RevenuePerUnit AS CASE WHEN Paid=0 THEN 0 ELSE CAST(Revenue/Paid AS decimal(21, 6)) END");
            Sql("ALTER TABLE cake.CampSum ADD CostPerUnit AS CASE WHEN Paid=0 THEN 0 ELSE CAST(Cost/Paid AS decimal(21, 6)) END");
        }
        
        public override void Down()
        {
            DropTable("cake.CampSum");
        }
    }
}
