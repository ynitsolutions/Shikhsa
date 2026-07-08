using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbasedetailsinstudentreg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegClassId",
                table: "Tbl_StudentsRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "Tbl_StudentDocument",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "Tbl_StudentDocument",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tbl_StudentDocument",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Tbl_StudentDocument",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Tbl_StudentDocument",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "Tbl_PreviousSchoolRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "Tbl_PreviousSchoolRecord",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tbl_PreviousSchoolRecord",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Tbl_PreviousSchoolRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Tbl_PreviousSchoolRecord",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "Tbl_Parents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "Tbl_Parents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tbl_Parents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Tbl_Parents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Tbl_Parents",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegClassId",
                table: "Tbl_StudentsRegistrations");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Tbl_StudentDocument");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Tbl_PreviousSchoolRecord");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "Tbl_Parents");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "Tbl_Parents");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tbl_Parents");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tbl_Parents");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Tbl_Parents");
        }
    }
}
