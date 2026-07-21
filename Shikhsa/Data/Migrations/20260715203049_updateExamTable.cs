using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateExamTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectIds",
                table: "scholasticExams");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "scholasticExams",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "scholasticExams");

            migrationBuilder.AddColumn<string>(
                name: "SubjectIds",
                table: "scholasticExams",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
