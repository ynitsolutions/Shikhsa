using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactionFeeDetail_StudentFees_StudentFeeId1",
                table: "PaymentTransactionFeeDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFeeCreditAdjustment_StudentFees_StudentFeeId1",
                table: "StudentFeeCreditAdjustment");

            migrationBuilder.DropIndex(
                name: "IX_StudentFeeCreditAdjustment_StudentFeeId1",
                table: "StudentFeeCreditAdjustment");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactionFeeDetail_StudentFeeId1",
                table: "PaymentTransactionFeeDetail");

            migrationBuilder.DropColumn(
                name: "StudentFeeId1",
                table: "StudentFeeCreditAdjustment");

            migrationBuilder.DropColumn(
                name: "StudentFeeId1",
                table: "PaymentTransactionFeeDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StudentFeeId1",
                table: "StudentFeeCreditAdjustment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StudentFeeId1",
                table: "PaymentTransactionFeeDetail",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeCreditAdjustment_StudentFeeId1",
                table: "StudentFeeCreditAdjustment",
                column: "StudentFeeId1");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactionFeeDetail_StudentFeeId1",
                table: "PaymentTransactionFeeDetail",
                column: "StudentFeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactionFeeDetail_StudentFees_StudentFeeId1",
                table: "PaymentTransactionFeeDetail",
                column: "StudentFeeId1",
                principalTable: "StudentFees",
                principalColumn: "StudentFeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFeeCreditAdjustment_StudentFees_StudentFeeId1",
                table: "StudentFeeCreditAdjustment",
                column: "StudentFeeId1",
                principalTable: "StudentFees",
                principalColumn: "StudentFeeId");
        }
    }
}
