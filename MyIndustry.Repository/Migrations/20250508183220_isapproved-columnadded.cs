using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class isapprovedcolumnadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "Services",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "SellerInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "SellerInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "SellerInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebSiteUrl",
                table: "SellerInfos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "SellerInfos");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "SellerInfos");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "SellerInfos");

            migrationBuilder.DropColumn(
                name: "WebSiteUrl",
                table: "SellerInfos");
        }
    }
}
