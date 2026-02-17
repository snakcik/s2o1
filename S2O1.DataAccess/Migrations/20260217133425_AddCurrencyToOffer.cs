using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S2O1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Offers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "OfferItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "OfferItems");
        }
    }
}
