namespace _3sem_lab10.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeStockFieldsTypeOTKAT : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Stocks", "PriceToday", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Stocks", "PriceYesterday", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Stocks", "PriceYesterday", c => c.Double(nullable: false));
            AlterColumn("dbo.Stocks", "PriceToday", c => c.Double(nullable: false));
        }
    }
}
