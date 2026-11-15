using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentGatewayValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "PaymentGatewaySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PaymentGatewaySettingValues",
                columns: table => new
                {
                    PaymentGatewaySettingValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentGatewaySettingId = table.Column<int>(type: "int", nullable: false),
                    SettingKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSecret = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGatewaySettingValues", x => x.PaymentGatewaySettingValueId);
                    table.ForeignKey(
                        name: "FK_PaymentGatewaySettingValues_PaymentGatewaySettings_PaymentGatewaySettingId",
                        column: x => x.PaymentGatewaySettingId,
                        principalTable: "PaymentGatewaySettings",
                        principalColumn: "PaymentGatewaySettingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewaySettings_GatewayName",
                table: "PaymentGatewaySettings",
                column: "GatewayName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewaySettingValues_PaymentGatewaySettingId_SettingKey",
                table: "PaymentGatewaySettingValues",
                columns: new[] { "PaymentGatewaySettingId", "SettingKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentGatewaySettingValues");

            migrationBuilder.DropIndex(
                name: "IX_PaymentGatewaySettings_GatewayName",
                table: "PaymentGatewaySettings");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PaymentGatewaySettings");
        }
    }
}
