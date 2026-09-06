using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyPaymentLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "PaymentTransactionDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchName",
                table: "PaymentTransactionDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "PaymentTransactionDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "PaymentTransactionDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "PaymentTransactionDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "PaymentTransactionDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "PaymentTransactionDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentName",
                table: "PaymentTransactionDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusCheckUrl",
                table: "PaymentGatewaySettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "BatchName",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "StudentName",
                table: "PaymentTransactionDetail");

            migrationBuilder.DropColumn(
                name: "StatusCheckUrl",
                table: "PaymentGatewaySettings");
        }
    }
}
