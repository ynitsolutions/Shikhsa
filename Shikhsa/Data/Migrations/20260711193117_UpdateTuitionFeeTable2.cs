using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTuitionFeeTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "TuitionFeePlans");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "TuitionFeePlans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "TuitionFeePlans",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "TuitionFeePlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId");
        }
    }
}
