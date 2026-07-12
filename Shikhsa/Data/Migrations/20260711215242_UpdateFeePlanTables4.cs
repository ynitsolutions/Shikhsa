using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFeePlanTables4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HostelFeePlans_FeeHeadings_FeeHeadingId",
                table: "HostelFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportFeePlans_Batches_BatchId",
                table: "TransportFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportFeePlans_FeeHeadings_FeeHeadingId",
                table: "TransportFeePlans");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "TransportFeePlans");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "TransportFeePlans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HostelFeePlans_FeeHeadings_FeeHeadingId",
                table: "HostelFeePlans",
                column: "FeeHeadingId",
                principalTable: "FeeHeadings",
                principalColumn: "FeeHeadingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportFeePlans_Batches_BatchId",
                table: "TransportFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportFeePlans_FeeHeadings_FeeHeadingId",
                table: "TransportFeePlans",
                column: "FeeHeadingId",
                principalTable: "FeeHeadings",
                principalColumn: "FeeHeadingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HostelFeePlans_FeeHeadings_FeeHeadingId",
                table: "HostelFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportFeePlans_Batches_BatchId",
                table: "TransportFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportFeePlans_FeeHeadings_FeeHeadingId",
                table: "TransportFeePlans");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "TransportFeePlans",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "TransportFeePlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_HostelFeePlans_FeeHeadings_FeeHeadingId",
                table: "HostelFeePlans",
                column: "FeeHeadingId",
                principalTable: "FeeHeadings",
                principalColumn: "FeeHeadingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportFeePlans_Batches_BatchId",
                table: "TransportFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportFeePlans_FeeHeadings_FeeHeadingId",
                table: "TransportFeePlans",
                column: "FeeHeadingId",
                principalTable: "FeeHeadings",
                principalColumn: "FeeHeadingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
