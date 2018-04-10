namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Sales : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "screen.Salesperson",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "screen.SalespersonStat",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        SalespersonId = c.Int(nullable: false),
                        EmailSent = c.Int(nullable: false),
                        EmailTracked = c.Int(nullable: false),
                        EmailOpened = c.Int(nullable: false),
                        EmailReplied = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Date, t.SalespersonId })
                .ForeignKey("screen.Salesperson", t => t.SalespersonId, cascadeDelete: true)
                .Index(t => t.SalespersonId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("screen.SalespersonStat", "SalespersonId", "screen.Salesperson");
            DropIndex("screen.SalespersonStat", new[] { "SalespersonId" });
            DropTable("screen.SalespersonStat");
            DropTable("screen.Salesperson");
        }
    }
}
