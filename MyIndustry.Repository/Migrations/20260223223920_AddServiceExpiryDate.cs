using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceExpiryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Services",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Services");
        }
    }
}
