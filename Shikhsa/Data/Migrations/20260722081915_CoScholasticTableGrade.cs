using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class CoScholasticTableGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoScholasticGrades",
                columns: table => new
                {
                    GradeEntryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ExamCategoryId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    CoScholasticAreaId = table.Column<long>(type: "bigint", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoScholasticGrades", x => x.GradeEntryId);
                    table.ForeignKey(
                        name: "FK_CoScholasticGrades_CoScholasticAreas_CoScholasticAreaId",
                        column: x => x.CoScholasticAreaId,
                        principalTable: "CoScholasticAreas",
                        principalColumn: "CoScholasticAreaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoScholasticGrades_Tbl_StudentsRegistrations_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_StudentsRegistrations",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoScholasticGrades_CoScholasticAreaId",
                table: "CoScholasticGrades",
                column: "CoScholasticAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CoScholasticGrades_StudentId",
                table: "CoScholasticGrades",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoScholasticGrades");
        }
    }
}
