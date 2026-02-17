using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Omar.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_UserId",
                table: "Products"
            );

            migrationBuilder.DropIndex(name: "IX_Products_UserId", table: "Products");

            migrationBuilder.DropColumn(name: "Quantity", table: "Products");

            migrationBuilder.DropColumn(name: "UserId", table: "Products");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SaleItems",
                newName: "SellingPrice"
            );

            migrationBuilder.RenameColumn(name: "Price", table: "Products", newName: "BuyingPrice");

            migrationBuilder.AlterColumn<int>(
                name: "MovementType",
                table: "StockMovements",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SaleItems",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)"
            );

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "StockQuantity",
                table: "Products",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)"
            );

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Expenses");

            migrationBuilder.DropIndex(name: "IX_Products_Barcode", table: "Products");

            migrationBuilder.DropColumn(name: "PurchasePrice", table: "SaleItems");

            migrationBuilder.DropColumn(name: "Barcode", table: "Products");

            migrationBuilder.RenameColumn(
                name: "SellingPrice",
                table: "SaleItems",
                newName: "Price"
            );

            migrationBuilder.RenameColumn(name: "BuyingPrice", table: "Products", newName: "Price");

            migrationBuilder.AlterColumn<string>(
                name: "MovementType",
                table: "StockMovements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "StockQuantity",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)"
            );

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "IX_Products_UserId",
                table: "Products",
                column: "UserId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_UserId",
                table: "Products",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
