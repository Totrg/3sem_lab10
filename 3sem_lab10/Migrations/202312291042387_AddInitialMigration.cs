namespace _3sem_lab10.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Stocks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Ticker = c.String(),
                        PriceToday = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PriceYesterday = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Stocks");
        }
    }
}
