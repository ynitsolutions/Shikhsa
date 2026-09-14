using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class TCAndCC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    BookNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SerialNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdmissionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StudentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DobInWords = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FirstAdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstAdmissionClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastClassFigures = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastClassWords = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastExamResult = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FailedDetails = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SubjectsStudied = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QualifiedForPromotion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PromotedClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FeePaidUpto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FeeConcession = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TotalWorkingDays = table.Column<int>(type: "int", nullable: true),
                    TotalPresentDays = table.Column<int>(type: "int", nullable: true),
                    NccScoutGuide = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ExtraCurricular = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GeneralConduct = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReasonForLeaving = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PurposeOfIssue = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CharacterRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}
