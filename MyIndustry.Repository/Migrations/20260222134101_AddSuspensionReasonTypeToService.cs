using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspensionReasonTypeToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SuspensionReasonDescription",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuspensionReasonType",
                table: "Services",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuspensionReasonDescription",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SuspensionReasonType",
                table: "Services");
        }
    }
}
