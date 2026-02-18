using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class relationsadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_SellerInfos_SellerInfoId",
                table: "Sellers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_SubCategories_SubCategoryId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Commissions");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "SellerInfoId",
                table: "Sellers",
                newName: "SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Sellers_SellerInfoId",
                table: "Sellers",
                newName: "IX_Sellers_SubscriptionPlanId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubCategoryId",
                table: "Services",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "SellerInfos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ServiceViewLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    ViewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceViewLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceViewLog_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionCampaign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: true),
                    FixedDiscountPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    CouponCode = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    IsOneTime = table.Column<bool>(type: "boolean", nullable: false),
                    TargetAudienceTag = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionCampaign", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SubscriptionType = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MonthlyPostLimit = table.Column<int>(type: "integer", nullable: false),
                    PostDurationInDays = table.Column<int>(type: "integer", nullable: false),
                    FeaturedPostLimit = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignUsage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignUsage_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignUsage_SubscriptionCampaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "SubscriptionCampaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_SellerId",
                table: "Services",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerInfos_SellerId",
                table: "SellerInfos",
                column: "SellerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsage_CampaignId",
                table: "CampaignUsage",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsage_SellerId",
                table: "CampaignUsage",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceViewLog_ServiceId",
                table: "ServiceViewLog",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerInfos_Sellers_SellerId",
                table: "SellerInfos",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_SubscriptionPlan_SubscriptionPlanId",
                table: "Sellers",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Sellers_SellerId",
                table: "Services",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_SubCategories_SubCategoryId",
                table: "Services",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerInfos_Sellers_SellerId",
                table: "SellerInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_SubscriptionPlan_SubscriptionPlanId",
                table: "Sellers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Sellers_SellerId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_SubCategories_SubCategoryId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "CampaignUsage");

            migrationBuilder.DropTable(
                name: "ServiceViewLog");

            migrationBuilder.DropTable(
                name: "SubscriptionPlan");

            migrationBuilder.DropTable(
                name: "SubscriptionCampaign");

            migrationBuilder.DropIndex(
                name: "IX_Services_SellerId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_SellerInfos_SellerId",
                table: "SellerInfos");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "SellerInfos");

            migrationBuilder.RenameColumn(
                name: "SubscriptionPlanId",
                table: "Sellers",
                newName: "SellerInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Sellers_SubscriptionPlanId",
                table: "Sellers",
                newName: "IX_Sellers_SellerInfoId");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubCategoryId",
                table: "Services",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Services",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsOpenContract = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    PurchaserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Purchasers_PurchaserId",
                        column: x => x.PurchaserId,
                        principalTable: "Purchasers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contracts_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    ServiceAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commissions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_ContractId",
                table: "Commissions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PurchaserId",
                table: "Contracts",
                column: "PurchaserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SellerId",
                table: "Contracts",
                column: "SellerId");

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
    }
}
