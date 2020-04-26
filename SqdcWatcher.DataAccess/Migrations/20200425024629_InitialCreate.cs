#region

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace XFactory.SqdcWatcher.DataAccess.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    ProducerName = table.Column<string>(nullable: true),
                    LevelTwoCategory = table.Column<string>(nullable: true),
                    CannabisType = table.Column<string>(nullable: true),
                    Strain = table.Column<string>(nullable: true),
                    Quality = table.Column<string>(nullable: true),
                    TerpeneDetailed = table.Column<string>(nullable: true),
                    Brand = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Products", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    ProductId = table.Column<string>(nullable: false),
                    InStock = table.Column<bool>(nullable: false),
                    GramEquivalent = table.Column<double>(nullable: false),
                    LastInStockTimestamp = table.Column<DateTime>(nullable: true),
                    PricePerGram = table.Column<double>(nullable: false),
                    DisplayPrice = table.Column<double>(nullable: false),
                    ListPrice = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecificationAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductVariantId = table.Column<long>(nullable: false),
                    PropertyName = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecificationAttribute_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationAttribute_ProductVariantId",
                table: "SpecificationAttribute",
                column: "ProductVariantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecificationAttribute");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}