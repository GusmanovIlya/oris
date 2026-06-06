using EfCoreHomework.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EfCoreHomework.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20240328000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "Name" },
            values: new object[,]
            {
                { 1, "Ноутбуки" },
                { 2, "Смартфоны" },
                { 3, "Аксессуары" }
            });

        migrationBuilder.InsertData(
            table: "Products",
            columns: new[] { "Id", "CategoryId", "Name", "Price" },
            values: new object[,]
            {
                { 1, 1, "Lenovo IdeaPad 3", 55000m },
                { 2, 1, "Apple MacBook Air", 120000m },
                { 3, 2, "Samsung Galaxy S23", 85000m },
                { 4, 2, "iPhone 14", 95000m },
                { 5, 3, "Logitech Mouse", 2500m },
                { 6, 3, "USB-C Cable", 900m }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryId",
            table: "Products",
            column: "CategoryId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Categories");
    }
}
