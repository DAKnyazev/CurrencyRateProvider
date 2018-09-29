using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CurrencyRateProvider.DbFiller.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currency",
                columns: table => new
                {
                    id = table.Column<short>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    code = table.Column<string>(maxLength: 3, nullable: true),
                    amount = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rate",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    relative_currency_id = table.Column<short>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    cost = table.Column<decimal>(nullable: false),
                    currency_id = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate", x => x.id);
                    table.ForeignKey(
                        name: "FK_rate_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rate_currency_relative_currency_id",
                        column: x => x.relative_currency_id,
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_currency_code_amount",
                table: "currency",
                columns: new[] { "code", "amount" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rate_currency_id",
                table: "rate",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_rate_relative_currency_id",
                table: "rate",
                column: "relative_currency_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rate");

            migrationBuilder.DropTable(
                name: "currency");
        }
    }
}
