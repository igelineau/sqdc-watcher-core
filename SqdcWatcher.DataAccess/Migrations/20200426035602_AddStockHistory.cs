#region

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace XFactory.SqdcWatcher.DataAccess.Migrations
{
    public partial class AddStockHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockHistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductVariantId = table.Column<long>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_StockHistory", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockHistory");
        }
    }
}