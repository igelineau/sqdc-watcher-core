using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XFactory.SqdcWatcher.DataAccess.Migrations
{
    public partial class AddPriceHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VariantId = table.Column<long>(nullable: false),
                    ProductVariantId = table.Column<long>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    NewDisplayPrice = table.Column<double>(nullable: true),
                    NewListPrice = table.Column<double>(nullable: true),
                    OldListPrice = table.Column<double>(nullable: true),
                    OldDisplayPrice = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistory_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_ProductVariantId",
                table: "PriceHistory",
                column: "ProductVariantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceHistory");
        }
    }
}