namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Conv : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.ConvCity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CountryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.ConvCountry", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId);
            
            CreateTable(
                "td.ConvCountry",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "td.Conv",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StrategyId = c.Int(),
                        TDadId = c.Int(),
                        Time = c.DateTime(nullable: false),
                        ConvType = c.String(maxLength: 25),
                        ConvVal = c.Decimal(nullable: false, precision: 18, scale: 6),
                        CityId = c.Int(),
                        IP = c.String(),
                        ExtData = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.ConvCity", t => t.CityId)
                .ForeignKey("td.Strategy", t => t.StrategyId)
                .ForeignKey("td.Ad", t => t.TDadId)
                .Index(t => t.StrategyId)
                .Index(t => t.TDadId)
                .Index(t => t.CityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.Conv", "TDadId", "td.Ad");
            DropForeignKey("td.Conv", "StrategyId", "td.Strategy");
            DropForeignKey("td.Conv", "CityId", "td.ConvCity");
            DropForeignKey("td.ConvCity", "CountryId", "td.ConvCountry");
            DropIndex("td.Conv", new[] { "CityId" });
            DropIndex("td.Conv", new[] { "TDadId" });
            DropIndex("td.Conv", new[] { "StrategyId" });
            DropIndex("td.ConvCity", new[] { "CountryId" });
            DropTable("td.Conv");
            DropTable("td.ConvCountry");
            DropTable("td.ConvCity");
        }
    }
}
