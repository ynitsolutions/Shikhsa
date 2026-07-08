using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "Tbl_StudentsRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StudentDocument_StudentId",
                table: "Tbl_StudentDocument",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_PreviousSchoolRecord_StudentId",
                table: "Tbl_PreviousSchoolRecord",
                column: "StudentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_PreviousSchoolRecord_Tbl_StudentsRegistrations_StudentId",
                table: "Tbl_PreviousSchoolRecord",
                column: "StudentId",
                principalTable: "Tbl_StudentsRegistrations",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tbl_StudentDocument_Tbl_StudentsRegistrations_StudentId",
                table: "Tbl_StudentDocument",
                column: "StudentId",
                principalTable: "Tbl_StudentsRegistrations",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_PreviousSchoolRecord_Tbl_StudentsRegistrations_StudentId",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_Tbl_StudentDocument_Tbl_StudentsRegistrations_StudentId",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_StudentDocument_StudentId",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_PreviousSchoolRecord_StudentId",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Tbl_StudentsRegistrations");
        }
    }
}
