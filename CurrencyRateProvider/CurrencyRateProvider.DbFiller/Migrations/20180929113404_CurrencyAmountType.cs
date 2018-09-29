using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyRateProvider.DbFiller.Migrations
{
    public partial class CurrencyAmountType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "amount",
                table: "currency",
                nullable: false,
                oldClrType: typeof(short));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "amount",
                table: "currency",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
