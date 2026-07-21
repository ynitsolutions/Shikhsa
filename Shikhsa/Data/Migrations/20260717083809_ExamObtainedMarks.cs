using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExamObtainedMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamObtainedMarks",
                columns: table => new
                {
                    ExamObtainedMarkId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    ObtainedMarks = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsAbsent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsFreeze = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamObtainedMarks", x => x.ExamObtainedMarkId);
                    table.ForeignKey(
                        name: "FK_ExamObtainedMarks_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamObtainedMarks_SubjectMasters_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "SubjectMasters",
                        principalColumn: "SubjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamObtainedMarks_Tbl_StudentsRegistrations_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Tbl_StudentsRegistrations",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamObtainedMarks_scholasticExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "scholasticExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_scholasticExams_BatchId",
                table: "scholasticExams",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_scholasticExams_ClassId",
                table: "scholasticExams",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_scholasticExams_ExamCategoryId",
                table: "scholasticExams",
                column: "ExamCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_scholasticExams_SubjectId",
                table: "scholasticExams",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamObtainedMarks_BatchId",
                table: "ExamObtainedMarks",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamObtainedMarks_ExamId",
                table: "ExamObtainedMarks",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamObtainedMarks_StudentId",
                table: "ExamObtainedMarks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamObtainedMarks_SubjectId",
                table: "ExamObtainedMarks",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_scholasticExams_Batches_BatchId",
                table: "scholasticExams",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_scholasticExams_DataListItems_ClassId",
                table: "scholasticExams",
                column: "ClassId",
                principalTable: "DataListItems",
                principalColumn: "DataListItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_scholasticExams_ExamCategories_ExamCategoryId",
                table: "scholasticExams",
                column: "ExamCategoryId",
                principalTable: "ExamCategories",
                principalColumn: "ExamCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_scholasticExams_SubjectMasters_SubjectId",
                table: "scholasticExams",
                column: "SubjectId",
                principalTable: "SubjectMasters",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_scholasticExams_Batches_BatchId",
                table: "scholasticExams");

            migrationBuilder.DropForeignKey(
                name: "FK_scholasticExams_DataListItems_ClassId",
                table: "scholasticExams");

            migrationBuilder.DropForeignKey(
                name: "FK_scholasticExams_ExamCategories_ExamCategoryId",
                table: "scholasticExams");

            migrationBuilder.DropForeignKey(
                name: "FK_scholasticExams_SubjectMasters_SubjectId",
                table: "scholasticExams");

            migrationBuilder.DropTable(
                name: "ExamObtainedMarks");

            migrationBuilder.DropIndex(
                name: "IX_scholasticExams_BatchId",
                table: "scholasticExams");

            migrationBuilder.DropIndex(
                name: "IX_scholasticExams_ClassId",
                table: "scholasticExams");

            migrationBuilder.DropIndex(
                name: "IX_scholasticExams_ExamCategoryId",
                table: "scholasticExams");

            migrationBuilder.DropIndex(
                name: "IX_scholasticExams_SubjectId",
                table: "scholasticExams");
        }
    }
}
