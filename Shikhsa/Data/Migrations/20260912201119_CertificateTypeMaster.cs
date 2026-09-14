using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class CertificateTypeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateType",
                table: "CertificateTemplates");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "GeneratedCertificates",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "CertificateTemplates",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "CertificateTemplates",
                newName: "AddedDate");

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "GeneratedCertificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedDate",
                table: "GeneratedCertificates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GeneratedCertificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubjectType",
                table: "GeneratedCertificates",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "GeneratedCertificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "GeneratedCertificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedBy",
                table: "CertificateTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CertificateTypeId",
                table: "CertificateTemplates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "CertificateTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CertificateTypes",
                columns: table => new
                {
                    CertificateTypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AppliesTo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTypes", x => x.CertificateTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_CertificateTypeId",
                table: "CertificateTemplates",
                column: "CertificateTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateTemplates_CertificateTypes_CertificateTypeId",
                table: "CertificateTemplates",
                column: "CertificateTypeId",
                principalTable: "CertificateTypes",
                principalColumn: "CertificateTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CertificateTemplates_CertificateTypes_CertificateTypeId",
                table: "CertificateTemplates");

            migrationBuilder.DropTable(
                name: "CertificateTypes");

            migrationBuilder.DropIndex(
                name: "IX_CertificateTemplates_CertificateTypeId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "GeneratedCertificates");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "GeneratedCertificates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GeneratedCertificates");

            migrationBuilder.DropColumn(
                name: "SubjectType",
                table: "GeneratedCertificates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "GeneratedCertificates");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "GeneratedCertificates");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "CertificateTypeId",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "CertificateTemplates");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "GeneratedCertificates",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "CertificateTemplates",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "AddedDate",
                table: "CertificateTemplates",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<string>(
                name: "CertificateType",
                table: "CertificateTemplates",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
