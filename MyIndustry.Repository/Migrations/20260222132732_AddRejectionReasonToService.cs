using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReasonDescription",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionReasonType",
                table: "Services",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReasonDescription",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "RejectionReasonType",
                table: "Services");
        }
    }
}
