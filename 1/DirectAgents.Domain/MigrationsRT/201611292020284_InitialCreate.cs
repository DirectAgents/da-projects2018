namespace DirectAgents.Domain.MigrationsRT
{
    using System;
    using System.Data.Entity.Migrations;
    using DirectAgents.Domain.Contexts;
    
    public partial class InitialCreate : DbMigration
    {
        private const string indexCode = "IX_UQ_Code";
        private const string tableProgVendor = RevTrackContext.extSchema + "." + RevTrackContext.tblProgVendor;
        private const string columnCode = "Code";

        public override void Up()
        {
            CreateTable(
                "ext.ProgClient",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "ext.ProgVendor",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        Code = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);

            Sql(string.Format(@"
                CREATE UNIQUE NONCLUSTERED INDEX {0}
                ON {1}({2})
                WHERE {2} IS NOT NULL;",
                indexCode, tableProgVendor, columnCode));
        }
        
        public override void Down()
        {
            DropIndex(tableProgVendor, indexCode);
            DropTable("ext.ProgVendor");
            DropTable("ext.ProgClient");
        }
    }
}
