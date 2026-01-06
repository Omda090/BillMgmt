namespace BillMgmt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sec : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Customers", "CustomerName", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.Stores", "StoreName", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.Products", "ProductName", c => c.String(nullable: false, maxLength: 150));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "ProductName", c => c.String());
            AlterColumn("dbo.Stores", "StoreName", c => c.String());
            AlterColumn("dbo.Customers", "CustomerName", c => c.String());
        }
    }
}
