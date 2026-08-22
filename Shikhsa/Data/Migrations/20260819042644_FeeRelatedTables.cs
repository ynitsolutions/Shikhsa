using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FeeRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTransactionDetail",
                columns: table => new
                {
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentModeId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TxnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Card = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Member = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayPaymentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewaySignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentCompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactionDetail", x => x.PaymentTransactionId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactionDetail_DataListItems_PaymentModeId",
                        column: x => x.PaymentModeId,
                        principalTable: "DataListItems",
                        principalColumn: "DataListItemId");
                    table.ForeignKey(
                        name: "FK_PaymentTransactionDetail_Tbl_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "StudentFees",
                columns: table => new
                {
                    StudentFeeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeeId = table.Column<long>(type: "bigint", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsFullyPaid = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFees", x => x.StudentFeeId);
                    table.ForeignKey(
                        name: "FK_StudentFees_Tbl_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "FeeReceipt",
                columns: table => new
                {
                    FeeReceiptId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    PaymentModeId = table.Column<int>(type: "int", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LateFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Concession = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConcessionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentYear = table.Column<int>(type: "int", nullable: false),
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeReceipt", x => x.FeeReceiptId);
                    table.ForeignKey(
                        name: "FK_FeeReceipt_DataListItems_PaymentModeId",
                        column: x => x.PaymentModeId,
                        principalTable: "DataListItems",
                        principalColumn: "DataListItemId");
                    table.ForeignKey(
                        name: "FK_FeeReceipt_PaymentTransactionDetail_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactionDetail",
                        principalColumn: "PaymentTransactionId");
                    table.ForeignKey(
                        name: "FK_FeeReceipt_Tbl_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactionFeeDetail",
                columns: table => new
                {
                    PaymentTransactionFeeDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    StudentFeeId = table.Column<long>(type: "bigint", nullable: false),
                    FeeId = table.Column<long>(type: "bigint", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactionFeeDetail", x => x.PaymentTransactionFeeDetailId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactionFeeDetail_PaymentTransactionDetail_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactionDetail",
                        principalColumn: "PaymentTransactionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentTransactionFeeDetail_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "StudentFeeId");
                });

            migrationBuilder.CreateTable(
                name: "FeeReceiptDetail",
                columns: table => new
                {
                    FeeReceiptDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    StudentFeeId = table.Column<long>(type: "bigint", nullable: false),
                    FeeId = table.Column<long>(type: "bigint", nullable: false),
                    FeeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeReceiptDetail", x => x.FeeReceiptDetailId);
                    table.ForeignKey(
                        name: "FK_FeeReceiptDetail_FeeReceipt_FeeReceiptId",
                        column: x => x.FeeReceiptId,
                        principalTable: "FeeReceipt",
                        principalColumn: "FeeReceiptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeReceiptDetail_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "StudentFeeId");
                });

            migrationBuilder.CreateTable(
                name: "StudentFeeCredit",
                columns: table => new
                {
                    FeeCreditId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeeReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsFullyUsed = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeeCredit", x => x.FeeCreditId);
                    table.ForeignKey(
                        name: "FK_StudentFeeCredit_FeeReceipt_FeeReceiptId",
                        column: x => x.FeeReceiptId,
                        principalTable: "FeeReceipt",
                        principalColumn: "FeeReceiptId");
                    table.ForeignKey(
                        name: "FK_StudentFeeCredit_Tbl_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "StudentFeeCreditAdjustment",
                columns: table => new
                {
                    FeeCreditAdjustmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeCreditId = table.Column<long>(type: "bigint", nullable: false),
                    StudentFeeId = table.Column<long>(type: "bigint", nullable: false),
                    FeeReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeeCreditAdjustment", x => x.FeeCreditAdjustmentId);
                    table.ForeignKey(
                        name: "FK_StudentFeeCreditAdjustment_FeeReceipt_FeeReceiptId",
                        column: x => x.FeeReceiptId,
                        principalTable: "FeeReceipt",
                        principalColumn: "FeeReceiptId");
                    table.ForeignKey(
                        name: "FK_StudentFeeCreditAdjustment_StudentFeeCredit_FeeCreditId",
                        column: x => x.FeeCreditId,
                        principalTable: "StudentFeeCredit",
                        principalColumn: "FeeCreditId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentFeeCreditAdjustment_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "StudentFeeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipt_PaymentModeId",
                table: "FeeReceipt",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipt_PaymentTransactionId",
                table: "FeeReceipt",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipt_StudentId",
                table: "FeeReceipt",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceiptDetail_FeeReceiptId",
                table: "FeeReceiptDetail",
                column: "FeeReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceiptDetail_StudentFeeId",
                table: "FeeReceiptDetail",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactionDetail_PaymentModeId",
                table: "PaymentTransactionDetail",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactionDetail_StudentId",
                table: "PaymentTransactionDetail",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactionFeeDetail_PaymentTransactionId",
                table: "PaymentTransactionFeeDetail",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactionFeeDetail_StudentFeeId",
                table: "PaymentTransactionFeeDetail",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeCredit_FeeReceiptId",
                table: "StudentFeeCredit",
                column: "FeeReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeCredit_StudentId",
                table: "StudentFeeCredit",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeCreditAdjustment_FeeCreditId",
                table: "StudentFeeCreditAdjustment",
                column: "FeeCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeCreditAdjustment_FeeReceiptId",
                table: "StudentFeeCreditAdjustment",
                column: "FeeReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeeCreditAdjustment_StudentFeeId",
                table: "StudentFeeCreditAdjustment",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_StudentId",
                table: "StudentFees",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeReceiptDetail");

            migrationBuilder.DropTable(
                name: "PaymentTransactionFeeDetail");

            migrationBuilder.DropTable(
                name: "StudentFeeCreditAdjustment");

            migrationBuilder.DropTable(
                name: "StudentFeeCredit");

            migrationBuilder.DropTable(
                name: "StudentFees");

            migrationBuilder.DropTable(
                name: "FeeReceipt");

            migrationBuilder.DropTable(
                name: "PaymentTransactionDetail");
        }
    }
}
