namespace DirectAgents.Domain.MigrationsAB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Vendor : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ab.Vendor",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("ab.SpendBit", "VendorId", c => c.Int(nullable: false));
            CreateIndex("ab.SpendBit", "VendorId");
            AddForeignKey("ab.SpendBit", "VendorId", "ab.Vendor", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("ab.SpendBit", "VendorId", "ab.Vendor");
            DropIndex("ab.SpendBit", new[] { "VendorId" });
            DropColumn("ab.SpendBit", "VendorId");
            DropTable("ab.Vendor");
        }
    }
}
