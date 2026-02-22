using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceLocationAndFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Condition",
                table: "Services",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ListingType",
                table: "Services",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Services",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ListingType",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Services");
        }
    }
}
