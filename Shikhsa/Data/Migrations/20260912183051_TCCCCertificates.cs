using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class TCCCCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateTemplates",
                columns: table => new
                {
                    CertificateTemplateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CertificateType = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplates", x => x.CertificateTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "CertificateTemplateCategories",
                columns: table => new
                {
                    CertificateTemplateCategoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    NotificationCategoryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplateCategories", x => x.CertificateTemplateCategoryId);
                    table.ForeignKey(
                        name: "FK_CertificateTemplateCategories_CertificateTemplates_CertificateTemplateId",
                        column: x => x.CertificateTemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "CertificateTemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificateTemplateCategories_NotificationCategories_NotificationCategoryId",
                        column: x => x.NotificationCategoryId,
                        principalTable: "NotificationCategories",
                        principalColumn: "NotificationCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedCertificates",
                columns: table => new
                {
                    GeneratedCertificateId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: true),
                    ManualValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalBodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedCertificates", x => x.GeneratedCertificateId);
                    table.ForeignKey(
                        name: "FK_GeneratedCertificates_CertificateTemplates_CertificateTemplateId",
                        column: x => x.CertificateTemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "CertificateTemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplateCategories_CertificateTemplateId",
                table: "CertificateTemplateCategories",
                column: "CertificateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplateCategories_NotificationCategoryId",
                table: "CertificateTemplateCategories",
                column: "NotificationCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedCertificates_CertificateTemplateId",
                table: "GeneratedCertificates",
                column: "CertificateTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateTemplateCategories");

            migrationBuilder.DropTable(
                name: "GeneratedCertificates");

            migrationBuilder.DropTable(
                name: "CertificateTemplates");
        }
    }
}
