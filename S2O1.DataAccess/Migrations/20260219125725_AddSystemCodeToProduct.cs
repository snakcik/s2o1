using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S2O1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemCodeToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemCode",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AssignedDelivererUserId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeInDispatch",
                table: "InvoiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AssignedDelivererUserId",
                table: "Invoices",
                column: "AssignedDelivererUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Users_AssignedDelivererUserId",
                table: "Invoices",
                column: "AssignedDelivererUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Users_AssignedDelivererUserId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AssignedDelivererUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SystemCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AssignedDelivererUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IncludeInDispatch",
                table: "InvoiceItems");
        }
    }
}
