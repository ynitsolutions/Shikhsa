using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExamRemarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentExamSummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ExamCategoryId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RankInClass = table.Column<int>(type: "int", nullable: true),
                    IsFreeze = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExamSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentExamSummaries_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_StudentExamSummaries_DataListItems_ClassId",
                        column: x => x.ClassId,
                        principalTable: "DataListItems",
                        principalColumn: "DataListItemId");
                    table.ForeignKey(
                        name: "FK_StudentExamSummaries_DataListItems_SectionId",
                        column: x => x.SectionId,
                        principalTable: "DataListItems",
                        principalColumn: "DataListItemId");
                    table.ForeignKey(
                        name: "FK_StudentExamSummaries_ExamCategories_ExamCategoryId",
                        column: x => x.ExamCategoryId,
                        principalTable: "ExamCategories",
                        principalColumn: "ExamCategoryId");
                    table.ForeignKey(
                        name: "FK_StudentExamSummaries_Tbl_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_scholasticExams_ExamType",
                table: "scholasticExams",
                column: "ExamType");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamSummaries_BatchId_ClassId_SectionId_ExamCategoryId_StudentId",
                table: "StudentExamSummaries",
                columns: new[] { "BatchId", "ClassId", "SectionId", "ExamCategoryId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamSummaries_ClassId",
                table: "StudentExamSummaries",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamSummaries_ExamCategoryId",
                table: "StudentExamSummaries",
                column: "ExamCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamSummaries_SectionId",
                table: "StudentExamSummaries",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamSummaries_StudentId",
                table: "StudentExamSummaries",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_scholasticExams_DataListItems_ExamType",
                table: "scholasticExams",
                column: "ExamType",
                principalTable: "DataListItems",
                principalColumn: "DataListItemId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_scholasticExams_DataListItems_ExamType",
                table: "scholasticExams");

            migrationBuilder.DropTable(
                name: "StudentExamSummaries");

            migrationBuilder.DropIndex(
                name: "IX_scholasticExams_ExamType",
                table: "scholasticExams");
        }
    }
}
