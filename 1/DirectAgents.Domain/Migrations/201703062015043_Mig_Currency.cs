namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Currency : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Currency",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Abbr = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("cake.CampSum", "RevCurrId", c => c.Int(nullable: false));
            AddColumn("cake.CampSum", "CostCurrId", c => c.Int(nullable: false));
            CreateIndex("cake.CampSum", "RevCurrId");
            CreateIndex("cake.CampSum", "CostCurrId");
            AddForeignKey("cake.CampSum", "CostCurrId", "dbo.Currency", "Id");
            AddForeignKey("cake.CampSum", "RevCurrId", "dbo.Currency", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("cake.CampSum", "RevCurrId", "dbo.Currency");
            DropForeignKey("cake.CampSum", "CostCurrId", "dbo.Currency");
            DropIndex("cake.CampSum", new[] { "CostCurrId" });
            DropIndex("cake.CampSum", new[] { "RevCurrId" });
            DropColumn("cake.CampSum", "CostCurrId");
            DropColumn("cake.CampSum", "RevCurrId");
            DropTable("dbo.Currency");
        }
    }
}
