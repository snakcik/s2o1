using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S2O1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddQuickActionsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuickActionsJson",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuickActionsJson",
                table: "Users");
        }
    }
}
