using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTuitionFeeTable3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_FeeHeadings_FeeHeadingId",
                table: "TuitionFeePlans");

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_FeeHeadings_FeeHeadingId",
                table: "TuitionFeePlans",
                column: "FeeHeadingId",
                principalTable: "FeeHeadings",
                principalColumn: "FeeHeadingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_FeeHeadings_FeeHeadingId",
                table: "TuitionFeePlans");

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_FeeHeadings_FeeHeadingId",
                table: "TuitionFeePlans",
                column: "FeeHeadingId",
                principalTable: "FeeHeadings",
                principalColumn: "FeeHeadingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
