using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubjectclassBatchMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassBatchSubjectHeaders",
                columns: table => new
                {
                    HeaderId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassBatchSubjectHeaders", x => x.HeaderId);
                });

            migrationBuilder.CreateTable(
                name: "ClassBatchSubjectDetails",
                columns: table => new
                {
                    DetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeaderId = table.Column<long>(type: "bigint", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassBatchSubjectDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_ClassBatchSubjectDetails_ClassBatchSubjectHeaders_HeaderId",
                        column: x => x.HeaderId,
                        principalTable: "ClassBatchSubjectHeaders",
                        principalColumn: "HeaderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassBatchSubjectDetails_HeaderId",
                table: "ClassBatchSubjectDetails",
                column: "HeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassBatchSubjectDetails");

            migrationBuilder.DropTable(
                name: "ClassBatchSubjectHeaders");
        }
    }
}
