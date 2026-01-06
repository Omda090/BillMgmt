namespace BillMgmt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCustomer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "Email", c => c.String(maxLength: 200));
            AddColumn("dbo.Customers", "PhoneNumber", c => c.String(maxLength: 30));
            AddColumn("dbo.Customers", "City", c => c.String(maxLength: 80));
            AlterColumn("dbo.Customers", "CustomerName", c => c.String(nullable: false, maxLength: 150));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "CustomerName", c => c.String());
            DropColumn("dbo.Customers", "City");
            DropColumn("dbo.Customers", "PhoneNumber");
            DropColumn("dbo.Customers", "Email");
        }
    }
}
