using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyStudentsTableJoinApplicationNO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApplicationNo",
                table: "Tbl_StudentsRegistrations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationNo",
                table: "Tbl_Students",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationNo",
                table: "Tbl_StudentDocument",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationNo",
                table: "Tbl_PreviousSchoolRecord",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_StudentsRegistrations",
                column: "ApplicationNo");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Students_ApplicationNo",
                table: "Tbl_Students",
                column: "ApplicationNo");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StudentDocument_ApplicationNo",
                table: "Tbl_StudentDocument",
                column: "ApplicationNo");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PreviousSchoolRecord_ApplicationNo",
                table: "Tbl_PreviousSchoolRecord",
                column: "ApplicationNo");

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_PreviousSchoolRecord_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_PreviousSchoolRecord",
                column: "ApplicationNo",
                principalTable: "Tbl_StudentsRegistrations",
                principalColumn: "ApplicationNo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_StudentDocument_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_StudentDocument",
                column: "ApplicationNo",
                principalTable: "Tbl_StudentsRegistrations",
                principalColumn: "ApplicationNo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_Students_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_Students",
                column: "ApplicationNo",
                principalTable: "Tbl_StudentsRegistrations",
                principalColumn: "ApplicationNo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_PreviousSchoolRecord_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_StudentDocument_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_Students_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_Students");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Tbl_StudentsRegistrations_ApplicationNo",
                table: "Tbl_StudentsRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Students_ApplicationNo",
                table: "Tbl_Students");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_StudentDocument_ApplicationNo",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_PreviousSchoolRecord_ApplicationNo",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "ApplicationNo",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropColumn(
                name: "ApplicationNo",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationNo",
                table: "Tbl_StudentsRegistrations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationNo",
                table: "Tbl_Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
