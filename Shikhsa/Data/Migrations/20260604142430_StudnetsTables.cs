using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudnetsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TagLine",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolMotto",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ManagerName",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Tbl_Parents",
                columns: table => new
                {
                    ParentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherMiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherMiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianFirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianMiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianLastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotherEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Parents", x => x.ParentId);
                });

            migrationBuilder.CreateTable(
                name: "Tbl_StudentsRegistrations",
                columns: table => new
                {
                    StudentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AadhaarNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APAARId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PENNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocalAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PermanentAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    ReligionId = table.Column<int>(type: "int", nullable: true),
                    IsHandicap = table.Column<bool>(type: "bit", nullable: false),
                    HandicapDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentificationMark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdmissionBatchId = table.Column<int>(type: "int", nullable: false),
                    IsInitialClassAdmission = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_StudentsRegistrations", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Tbl_StudentsRegistrations_Tbl_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tbl_Parents",
                        principalColumn: "ParentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_StudentsRegistrations_ParentId",
                table: "Tbl_StudentsRegistrations",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_StudentsRegistrations");

            migrationBuilder.DropTable(
                name: "Tbl_Parents");

            migrationBuilder.AlterColumn<string>(
                name: "TagLine",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolMotto",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ManagerName",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "SchoolMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
