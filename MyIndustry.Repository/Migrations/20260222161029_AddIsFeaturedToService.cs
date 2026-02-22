using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFeaturedToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Services");
        }
    }
}
