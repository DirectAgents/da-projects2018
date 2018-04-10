namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_UnitType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.UnitType",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        Abbrev = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("ab.SpendBit", "UnitTypeId", c => c.Int(nullable: false));
            CreateIndex("ab.SpendBit", "UnitTypeId");
            AddForeignKey("ab.SpendBit", "UnitTypeId", "ab.UnitType", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("ab.SpendBit", "UnitTypeId", "ab.UnitType");
            DropIndex("ab.SpendBit", new[] { "UnitTypeId" });
            DropColumn("ab.SpendBit", "UnitTypeId");
            DropTable("ab.UnitType");
        }
    }
}
