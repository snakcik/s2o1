using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S2O1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SyncInvoiceAssignedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "WarehouseAssignedDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarehouseCompletedDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarehouseIncompleteDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceStatusLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreparedItemsCount = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStatusLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceStatusLogs_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceStatusLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStatusLogs_InvoiceId",
                table: "InvoiceStatusLogs",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStatusLogs_UserId",
                table: "InvoiceStatusLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceStatusLogs");

            migrationBuilder.DropColumn(
                name: "WarehouseAssignedDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WarehouseCompletedDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WarehouseIncompleteDate",
                table: "Invoices");
        }
    }
}
