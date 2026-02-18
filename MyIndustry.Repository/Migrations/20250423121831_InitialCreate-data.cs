using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatedata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionId",
                table: "Contracts");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Services",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubCategoryId",
                table: "Services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SellerInfoId",
                table: "Sellers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "Commissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_SubCategoryId",
                table: "Services",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_SellerInfoId",
                table: "Sellers",
                column: "SellerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaserInfos_PurchaserId",
                table: "PurchaserInfos",
                column: "PurchaserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PurchaserId",
                table: "Contracts",
                column: "PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SellerId",
                table: "Contracts",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_ContractId",
                table: "Commissions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CategoryId",
                table: "SubCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commissions_Contracts_ContractId",
                table: "Commissions",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Purchasers_PurchaserId",
                table: "Contracts",
                column: "PurchaserId",
                principalTable: "Purchasers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Sellers_SellerId",
                table: "Contracts",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaserInfos_Purchasers_PurchaserId",
                table: "PurchaserInfos",
                column: "PurchaserId",
                principalTable: "Purchasers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_SellerInfos_SellerInfoId",
                table: "Sellers",
                column: "SellerInfoId",
                principalTable: "SellerInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_SubCategories_SubCategoryId",
                table: "Services",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commissions_Contracts_ContractId",
                table: "Commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Purchasers_PurchaserId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Sellers_SellerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaserInfos_Purchasers_PurchaserId",
                table: "PurchaserInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_SellerInfos_SellerInfoId",
                table: "Sellers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_SubCategories_SubCategoryId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Services_SubCategoryId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_SellerInfoId",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_PurchaserInfos_PurchaserId",
                table: "PurchaserInfos");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_PurchaserId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_SellerId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Commissions_ContractId",
                table: "Commissions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SellerInfoId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Commissions");

            migrationBuilder.AddColumn<Guid>(
                name: "CommissionId",
                table: "Contracts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
