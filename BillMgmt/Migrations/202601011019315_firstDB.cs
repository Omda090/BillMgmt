namespace BillMgmt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class firstDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BillItems",
                c => new
                    {
                        BillItemId = c.Int(nullable: false, identity: true),
                        BillId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.BillItemId)
                .ForeignKey("dbo.Bills", t => t.BillId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.BillId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Bills",
                c => new
                    {
                        BillId = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        StoreId = c.Int(nullable: false),
                        BillDate = c.DateTime(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.BillId)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        CustomerId = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(),
                    })
                .PrimaryKey(t => t.CustomerId);
            
            CreateTable(
                "dbo.Stores",
                c => new
                    {
                        StoreId = c.Int(nullable: false, identity: true),
                        StoreName = c.String(),
                    })
                .PrimaryKey(t => t.StoreId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        ProductName = c.String(),
                    })
                .PrimaryKey(t => t.ProductId);
            
            CreateTable(
                "dbo.StoreProducts",
                c => new
                    {
                        StoreProductId = c.Int(nullable: false, identity: true),
                        StoreId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        StockQty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.StoreProductId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.Stores", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId)
                .Index(t => t.ProductId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreProducts", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.StoreProducts", "ProductId", "dbo.Products");
            DropForeignKey("dbo.BillItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Bills", "StoreId", "dbo.Stores");
            DropForeignKey("dbo.BillItems", "BillId", "dbo.Bills");
            DropForeignKey("dbo.Bills", "CustomerId", "dbo.Customers");
            DropIndex("dbo.StoreProducts", new[] { "ProductId" });
            DropIndex("dbo.StoreProducts", new[] { "StoreId" });
            DropIndex("dbo.Bills", new[] { "StoreId" });
            DropIndex("dbo.Bills", new[] { "CustomerId" });
            DropIndex("dbo.BillItems", new[] { "ProductId" });
            DropIndex("dbo.BillItems", new[] { "BillId" });
            DropTable("dbo.StoreProducts");
            DropTable("dbo.Products");
            DropTable("dbo.Stores");
            DropTable("dbo.Customers");
            DropTable("dbo.Bills");
            DropTable("dbo.BillItems");
        }
    }
}
