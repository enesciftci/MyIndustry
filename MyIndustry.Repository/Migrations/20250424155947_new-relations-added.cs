using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyIndustry.Repository.Migrations
{
    /// <inheritdoc />
    public partial class newrelationsadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignUsage_SubscriptionCampaign_CampaignId",
                table: "CampaignUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_SubscriptionPlan_SubscriptionPlanId",
                table: "Sellers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceViewLog_Services_ServiceId",
                table: "ServiceViewLog");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_SubscriptionPlanId",
                table: "Sellers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlan",
                table: "SubscriptionPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionCampaign",
                table: "SubscriptionCampaign");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceViewLog",
                table: "ServiceViewLog");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "Sellers");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlan",
                newName: "SubscriptionPlans");

            migrationBuilder.RenameTable(
                name: "SubscriptionCampaign",
                newName: "SubscriptionCampaigns");

            migrationBuilder.RenameTable(
                name: "ServiceViewLog",
                newName: "ServiceViewLogs");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceViewLog_ServiceId",
                table: "ServiceViewLogs",
                newName: "IX_ServiceViewLogs_ServiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionCampaigns",
                table: "SubscriptionCampaigns",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceViewLogs",
                table: "ServiceViewLogs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SellerSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RemainingPostQuota = table.Column<int>(type: "integer", nullable: false),
                    RemainingFeaturedQuota = table.Column<int>(type: "integer", nullable: false),
                    IsAutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerSubscriptions_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SellerSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionRenewalHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RenewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PaymentProviderTransactionId = table.Column<string>(type: "text", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionRenewalHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellerSubscriptionRenewalHistory",
                columns: table => new
                {
                    SellersId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionRenewalHistoriesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerSubscriptionRenewalHistory", x => new { x.SellersId, x.SubscriptionRenewalHistoriesId });
                    table.ForeignKey(
                        name: "FK_SellerSubscriptionRenewalHistory_Sellers_SellersId",
                        column: x => x.SellersId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SellerSubscriptionRenewalHistory_SubscriptionRenewalHistori~",
                        column: x => x.SubscriptionRenewalHistoriesId,
                        principalTable: "SubscriptionRenewalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionCampaigns_SubscriptionPlanId",
                table: "SubscriptionCampaigns",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerSubscriptionRenewalHistory_SubscriptionRenewalHistori~",
                table: "SellerSubscriptionRenewalHistory",
                column: "SubscriptionRenewalHistoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerSubscriptions_SellerId",
                table: "SellerSubscriptions",
                column: "SellerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerSubscriptions_SubscriptionPlanId",
                table: "SellerSubscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignUsage_SubscriptionCampaigns_CampaignId",
                table: "CampaignUsage",
                column: "CampaignId",
                principalTable: "SubscriptionCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceViewLogs_Services_ServiceId",
                table: "ServiceViewLogs",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionCampaigns_SubscriptionPlans_SubscriptionPlanId",
                table: "SubscriptionCampaigns",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignUsage_SubscriptionCampaigns_CampaignId",
                table: "CampaignUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceViewLogs_Services_ServiceId",
                table: "ServiceViewLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionCampaigns_SubscriptionPlans_SubscriptionPlanId",
                table: "SubscriptionCampaigns");

            migrationBuilder.DropTable(
                name: "SellerSubscriptionRenewalHistory");

            migrationBuilder.DropTable(
                name: "SellerSubscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionRenewalHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionCampaigns",
                table: "SubscriptionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionCampaigns_SubscriptionPlanId",
                table: "SubscriptionCampaigns");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceViewLogs",
                table: "ServiceViewLogs");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlans",
                newName: "SubscriptionPlan");

            migrationBuilder.RenameTable(
                name: "SubscriptionCampaigns",
                newName: "SubscriptionCampaign");

            migrationBuilder.RenameTable(
                name: "ServiceViewLogs",
                newName: "ServiceViewLog");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceViewLogs_ServiceId",
                table: "ServiceViewLog",
                newName: "IX_ServiceViewLog_ServiceId");

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "Sellers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlan",
                table: "SubscriptionPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionCampaign",
                table: "SubscriptionCampaign",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceViewLog",
                table: "ServiceViewLog",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_SubscriptionPlanId",
                table: "Sellers",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignUsage_SubscriptionCampaign_CampaignId",
                table: "CampaignUsage",
                column: "CampaignId",
                principalTable: "SubscriptionCampaign",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_SubscriptionPlan_SubscriptionPlanId",
                table: "Sellers",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceViewLog_Services_ServiceId",
                table: "ServiceViewLog",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
