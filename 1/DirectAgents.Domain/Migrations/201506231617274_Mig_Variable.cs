namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mig_Variable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Variable",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        StringVal = c.String(),
                        IntVal = c.Int(),
                        DecVal = c.Decimal(precision: 18, scale: 6),
                    })
                .PrimaryKey(t => t.Name);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Variable");
        }
    }
}
