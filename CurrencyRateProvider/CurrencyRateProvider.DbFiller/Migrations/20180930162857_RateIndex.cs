using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyRateProvider.DbFiller.Migrations
{
    public partial class RateIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rate_currency_id",
                table: "rate");

            migrationBuilder.CreateIndex(
                name: "IX_rate_currency_id_relative_currency_id_date",
                table: "rate",
                columns: new[] { "currency_id", "relative_currency_id", "date" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rate_currency_id_relative_currency_id_date",
                table: "rate");

            migrationBuilder.CreateIndex(
                name: "IX_rate_currency_id",
                table: "rate",
                column: "currency_id");
        }
    }
}
