namespace DirectAgents.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CakeContactAndRole : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "cake.Contact",
                c => new
                    {
                        ContactId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        EmailAddress = c.String(),
                        Title = c.String(),
                        PhoneWork = c.String(),
                        PhoneCell = c.String(),
                        PhoneFax = c.String(),
                    })
                .PrimaryKey(t => t.ContactId)
                .ForeignKey("cake.Role", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "cake.Role",
                c => new
                    {
                        RoleId = c.Int(nullable: false),
                        RoleName = c.String(),
                    })
                .PrimaryKey(t => t.RoleId);
            
            AddColumn("cake.Advertiser", "AccountManagerId", c => c.Int());
            AddColumn("cake.Advertiser", "AdManagerId", c => c.Int());
            AddForeignKey("cake.Advertiser", "AccountManagerId", "cake.Contact", "ContactId");
            AddForeignKey("cake.Advertiser", "AdManagerId", "cake.Contact", "ContactId");
            CreateIndex("cake.Advertiser", "AccountManagerId");
            CreateIndex("cake.Advertiser", "AdManagerId");
        }
        
        public override void Down()
        {
            DropIndex("cake.Contact", new[] { "RoleId" });
            DropIndex("cake.Advertiser", new[] { "AdManagerId" });
            DropIndex("cake.Advertiser", new[] { "AccountManagerId" });
            DropForeignKey("cake.Contact", "RoleId", "cake.Role");
            DropForeignKey("cake.Advertiser", "AdManagerId", "cake.Contact");
            DropForeignKey("cake.Advertiser", "AccountManagerId", "cake.Contact");
            DropColumn("cake.Advertiser", "AdManagerId");
            DropColumn("cake.Advertiser", "AccountManagerId");
            DropTable("cake.Role");
            DropTable("cake.Contact");
        }
    }
}
