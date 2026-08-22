using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeHeadDefaultvalues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeeType",
                table: "FeeReceiptDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousBalanceAmount",
                table: "FeeReceiptDetail",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousPaidAmount",
                table: "FeeReceiptDetail",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "FeeHeadings",
                keyColumn: "FeeHeadingId",
                keyValue: 14L);

            migrationBuilder.DropColumn(
                name: "FeeType",
                table: "FeeReceiptDetail");

            migrationBuilder.DropColumn(
                name: "PreviousBalanceAmount",
                table: "FeeReceiptDetail");

            migrationBuilder.DropColumn(
                name: "PreviousPaidAmount",
                table: "FeeReceiptDetail");
        }
    }
}
