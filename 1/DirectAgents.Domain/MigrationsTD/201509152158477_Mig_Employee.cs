namespace DirectAgents.Domain.MigrationsTD
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Employee : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employee",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("td.Advertiser", "SalesRepId", c => c.Int());
            AddColumn("td.Advertiser", "AMId", c => c.Int());
            CreateIndex("td.Advertiser", "SalesRepId");
            CreateIndex("td.Advertiser", "AMId");
            AddForeignKey("td.Advertiser", "AMId", "dbo.Employee", "Id");
            AddForeignKey("td.Advertiser", "SalesRepId", "dbo.Employee", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("td.Advertiser", "SalesRepId", "dbo.Employee");
            DropForeignKey("td.Advertiser", "AMId", "dbo.Employee");
            DropIndex("td.Advertiser", new[] { "AMId" });
            DropIndex("td.Advertiser", new[] { "SalesRepId" });
            DropColumn("td.Advertiser", "AMId");
            DropColumn("td.Advertiser", "SalesRepId");
            DropTable("dbo.Employee");
        }
    }
}
