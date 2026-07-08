using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tbl_StudentsRegistrations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tbl_Students",
                columns: table => new
                {
                    StudentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AadhaarNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    APAARId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PENNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PermanentAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    GenderId = table.Column<int>(type: "int", nullable: true),
                    ReligionId = table.Column<int>(type: "int", nullable: true),
                    IsHandicap = table.Column<bool>(type: "bit", nullable: false),
                    HandicapDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentificationMark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdmissionBatchId = table.Column<int>(type: "int", nullable: true),
                    IsInitialClassAdmission = table.Column<bool>(type: "bit", nullable: true),
                    AdmitClassId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    AdmitSectionId = table.Column<int>(type: "int", nullable: true),
                    AdmitBatchId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentRegisterId = table.Column<long>(type: "bigint", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tbl_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Tbl_Students_Tbl_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tbl_Parents",
                        principalColumn: "ParentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Students_ParentId",
                table: "Tbl_Students",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tbl_Students");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tbl_StudentsRegistrations");
        }
    }
}
