using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S2O1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferRowVersionConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Customers_CustomerId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Offers");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Offers",
                type: "rowversion",
                rowVersion: true,
                nullable: true); // rowversion handles nullability differently, usually implicitly mandatory but generator handles it. Let's try nullable=true to avoid explicit value requirement during add if table has data? 
            // Actually, adding a non-nullable column to table with data requires default.
            // timestamp/rowversion automatically populates for existing rows! So nullable=true is probably safer or let it be.
            // But EF Core `IsRowVersion` generally maps to `timestamp`/`rowversion` which is NOT NULL.
            // Let's stick to nullable: true for safety, or check if rowVersion: true implies behavior.

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Customers_CustomerId",
                table: "Offers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Customers_CustomerId",
                table: "Offers");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Offers",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Customers_CustomerId",
                table: "Offers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
