namespace _3sem_lab10.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeStockFieldsType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Stocks", "PriceToday", c => c.Double(nullable: false));
            AlterColumn("dbo.Stocks", "PriceYesterday", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Stocks", "PriceYesterday", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Stocks", "PriceToday", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
