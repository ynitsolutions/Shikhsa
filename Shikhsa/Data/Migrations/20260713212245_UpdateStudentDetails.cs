using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HostelId",
                table: "Tbl_StudentsRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHostel",
                table: "Tbl_StudentsRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTranspot",
                table: "Tbl_StudentsRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TranspotId",
                table: "Tbl_StudentsRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HostelId",
                table: "Tbl_Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHostel",
                table: "Tbl_Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTranspot",
                table: "Tbl_Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ScholarNumber",
                table: "Tbl_Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TranspotId",
                table: "Tbl_Students",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostelId",
                table: "Tbl_StudentsRegistrations");

            migrationBuilder.DropColumn(
                name: "IsHostel",
                table: "Tbl_StudentsRegistrations");

            migrationBuilder.DropColumn(
                name: "IsTranspot",
                table: "Tbl_StudentsRegistrations");

            migrationBuilder.DropColumn(
                name: "TranspotId",
                table: "Tbl_StudentsRegistrations");

            migrationBuilder.DropColumn(
                name: "HostelId",
                table: "Tbl_Students");

            migrationBuilder.DropColumn(
                name: "IsHostel",
                table: "Tbl_Students");

            migrationBuilder.DropColumn(
                name: "IsTranspot",
                table: "Tbl_Students");

            migrationBuilder.DropColumn(
                name: "ScholarNumber",
                table: "Tbl_Students");

            migrationBuilder.DropColumn(
                name: "TranspotId",
                table: "Tbl_Students");
        }
    }
}
