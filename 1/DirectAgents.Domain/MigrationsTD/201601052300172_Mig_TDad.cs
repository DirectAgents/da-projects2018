namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_TDad : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.Ad",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        ExternalId = c.String(),
                        Name = c.String(),
                        Width = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Account", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "td.AdSummary",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        TDadId = c.Int(nullable: false),
                        Impressions = c.Int(nullable: false),
                        Clicks = c.Int(nullable: false),
                        PostClickConv = c.Int(nullable: false),
                        PostViewConv = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 6),
                    })
                .PrimaryKey(t => new { t.Date, t.TDadId })
                .ForeignKey("td.Ad", t => t.TDadId, cascadeDelete: true)
                .Index(t => t.TDadId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.AdSummary", "TDadId", "td.Ad");
            DropForeignKey("td.Ad", "AccountId", "td.Account");
            DropIndex("td.AdSummary", new[] { "TDadId" });
            DropIndex("td.Ad", new[] { "AccountId" });
            DropTable("td.AdSummary");
            DropTable("td.Ad");
        }
    }
}
