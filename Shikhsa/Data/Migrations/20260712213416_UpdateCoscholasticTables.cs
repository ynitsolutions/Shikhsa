using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoscholasticTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CoScholastics",
                table: "CoScholastics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoScholasticAreas",
                table: "CoScholasticAreas");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CoScholastics");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CoScholasticAreas");

            migrationBuilder.DropColumn(
                name: "SubjectNameInLanguage",
                table: "CoScholasticAreas");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CoScholasticAreas");

            migrationBuilder.AddColumn<long>(
                name: "CoScholasticId",
                table: "CoScholastics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "CoScholasticAreaId",
                table: "CoScholasticAreas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "CoScholasticId",
                table: "CoScholasticAreas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoScholastics",
                table: "CoScholastics",
                column: "CoScholasticId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoScholasticAreas",
                table: "CoScholasticAreas",
                column: "CoScholasticAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CoScholasticAreas_ClassId",
                table: "CoScholasticAreas",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CoScholasticAreas_CoScholasticId",
                table: "CoScholasticAreas",
                column: "CoScholasticId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoScholasticAreas_CoScholastics_CoScholasticId",
                table: "CoScholasticAreas",
                column: "CoScholasticId",
                principalTable: "CoScholastics",
                principalColumn: "CoScholasticId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CoScholasticAreas_DataListItems_ClassId",
                table: "CoScholasticAreas",
                column: "ClassId",
                principalTable: "DataListItems",
                principalColumn: "DataListItemId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoScholasticAreas_CoScholastics_CoScholasticId",
                table: "CoScholasticAreas");

            migrationBuilder.DropForeignKey(
                name: "FK_CoScholasticAreas_DataListItems_ClassId",
                table: "CoScholasticAreas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoScholastics",
                table: "CoScholastics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoScholasticAreas",
                table: "CoScholasticAreas");

            migrationBuilder.DropIndex(
                name: "IX_CoScholasticAreas_ClassId",
                table: "CoScholasticAreas");

            migrationBuilder.DropIndex(
                name: "IX_CoScholasticAreas_CoScholasticId",
                table: "CoScholasticAreas");

            migrationBuilder.DropColumn(
                name: "CoScholasticId",
                table: "CoScholastics");

            migrationBuilder.DropColumn(
                name: "CoScholasticAreaId",
                table: "CoScholasticAreas");

            migrationBuilder.DropColumn(
                name: "CoScholasticId",
                table: "CoScholasticAreas");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CoScholastics",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CoScholasticAreas",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "SubjectNameInLanguage",
                table: "CoScholasticAreas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CoScholasticAreas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoScholastics",
                table: "CoScholastics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoScholasticAreas",
                table: "CoScholasticAreas",
                column: "Id");
        }
    }
}
