using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class PaymentGatewaySetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Concession",
                table: "FeeReceipt",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "PaymentGatewaySettings",
                columns: table => new
                {
                    PaymentGatewaySettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GatewayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MerchId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MerchPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AuthUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckoutEnvironment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CheckoutCdn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestEncryptKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseDecryptKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseHashKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGatewaySettings", x => x.PaymentGatewaySettingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentGatewaySettings");

            migrationBuilder.AlterColumn<string>(
                name: "Concession",
                table: "FeeReceipt",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
