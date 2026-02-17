using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S2O1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierToPriceList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode_WarehouseId",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "PriceLists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode_WarehouseId",
                table: "Products",
                columns: new[] { "ProductCode", "WarehouseId" },
                unique: true,
                filter: "[WarehouseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_SupplierId",
                table: "PriceLists",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceLists_Suppliers_SupplierId",
                table: "PriceLists",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceLists_Suppliers_SupplierId",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode_WarehouseId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_PriceLists_SupplierId",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "PriceLists");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode_WarehouseId",
                table: "Products",
                columns: new[] { "ProductCode", "WarehouseId" },
                unique: true);
        }
    }
}
