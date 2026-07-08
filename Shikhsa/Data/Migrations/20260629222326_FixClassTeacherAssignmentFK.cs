using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixClassTeacherAssignmentFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_Batches_BatchId1",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_StaffMaster_StaffId",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_SubjectMasters_SubjectId1",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassTeacherSubjectAssignments_BatchId1",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassTeacherSubjectAssignments_SubjectId1",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropColumn(
                name: "BatchId1",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropColumn(
                name: "SubjectId1",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "SubjectId",
                table: "ClassTeacherSubjectAssignments",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "SectionId",
                table: "ClassTeacherSubjectAssignments",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "ClassId",
                table: "ClassTeacherSubjectAssignments",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "ClassTeacherSubjectAssignments",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "AttendanceTypes",
                columns: table => new
                {
                    AttendanceTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsLeave = table.Column<bool>(type: "bit", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceTypes", x => x.AttendanceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StaffAttendances",
                columns: table => new
                {
                    AttendanceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<long>(type: "bigint", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AttendanceTypeId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAttendances", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_StaffAttendances_AttendanceTypes_AttendanceTypeId",
                        column: x => x.AttendanceTypeId,
                        principalTable: "AttendanceTypes",
                        principalColumn: "AttendanceTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAttendances_StaffMaster_StaffId",
                        column: x => x.StaffId,
                        principalTable: "StaffMaster",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassTeacherSubjectAssignments_BatchId",
                table: "ClassTeacherSubjectAssignments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTeacherSubjectAssignments_SubjectId",
                table: "ClassTeacherSubjectAssignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAttendances_AttendanceTypeId",
                table: "StaffAttendances",
                column: "AttendanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAttendances_StaffId_AttendanceDate",
                table: "StaffAttendances",
                columns: new[] { "StaffId", "AttendanceDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_Batches_BatchId",
                table: "ClassTeacherSubjectAssignments",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_StaffMaster_StaffId",
                table: "ClassTeacherSubjectAssignments",
                column: "StaffId",
                principalTable: "StaffMaster",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_SubjectMasters_SubjectId",
                table: "ClassTeacherSubjectAssignments",
                column: "SubjectId",
                principalTable: "SubjectMasters",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_Batches_BatchId",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_StaffMaster_StaffId",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_SubjectMasters_SubjectId",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropTable(
                name: "StaffAttendances");

            migrationBuilder.DropTable(
                name: "AttendanceTypes");

            migrationBuilder.DropIndex(
                name: "IX_ClassTeacherSubjectAssignments_BatchId",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassTeacherSubjectAssignments_SubjectId",
                table: "ClassTeacherSubjectAssignments");

            migrationBuilder.AlterColumn<long>(
                name: "SubjectId",
                table: "ClassTeacherSubjectAssignments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "SectionId",
                table: "ClassTeacherSubjectAssignments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "ClassId",
                table: "ClassTeacherSubjectAssignments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "BatchId",
                table: "ClassTeacherSubjectAssignments",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BatchId1",
                table: "ClassTeacherSubjectAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId1",
                table: "ClassTeacherSubjectAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClassTeacherSubjectAssignments_BatchId1",
                table: "ClassTeacherSubjectAssignments",
                column: "BatchId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTeacherSubjectAssignments_SubjectId1",
                table: "ClassTeacherSubjectAssignments",
                column: "SubjectId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_Batches_BatchId1",
                table: "ClassTeacherSubjectAssignments",
                column: "BatchId1",
                principalTable: "Batches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_StaffMaster_StaffId",
                table: "ClassTeacherSubjectAssignments",
                column: "StaffId",
                principalTable: "StaffMaster",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTeacherSubjectAssignments_SubjectMasters_SubjectId1",
                table: "ClassTeacherSubjectAssignments",
                column: "SubjectId1",
                principalTable: "SubjectMasters",
                principalColumn: "SubjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
