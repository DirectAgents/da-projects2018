namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_PlatColMapping : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "td.PlatColMapping",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Date = c.String(nullable: false),
                        Cost = c.String(),
                        Impressions = c.String(),
                        Clicks = c.String(),
                        PostClickConv = c.String(),
                        PostViewConv = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("td.Platform", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("td.PlatColMapping", "Id", "td.Platform");
            DropIndex("td.PlatColMapping", new[] { "Id" });
            DropTable("td.PlatColMapping");
        }
    }
}
