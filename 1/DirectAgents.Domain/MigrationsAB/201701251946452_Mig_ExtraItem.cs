namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_ExtraItem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.ExtraItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Description = c.String(),
                        Revenue = c.Decimal(nullable: false, precision: 14, scale: 2),
                        Cost = c.Decimal(nullable: false, precision: 14, scale: 2),
                        JobId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("ab.Job", t => t.JobId, cascadeDelete: true)
                .Index(t => t.JobId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ab.ExtraItem", "JobId", "ab.Job");
            DropIndex("ab.ExtraItem", new[] { "JobId" });
            DropTable("ab.ExtraItem");
        }
    }
}
