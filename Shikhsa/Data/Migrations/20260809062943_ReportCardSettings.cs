using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReportCardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportCardSettings",
                columns: table => new
                {
                    ReportCardSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    BoardType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShowLogo = table.Column<bool>(type: "bit", nullable: false),
                    ShowStudentPhoto = table.Column<bool>(type: "bit", nullable: false),
                    ShowQRCode = table.Column<bool>(type: "bit", nullable: false),
                    ShowWatermark = table.Column<bool>(type: "bit", nullable: false),
                    ShowTeacherSignature = table.Column<bool>(type: "bit", nullable: false),
                    ShowPrincipalSignature = table.Column<bool>(type: "bit", nullable: false),
                    UseDigitalSignature = table.Column<bool>(type: "bit", nullable: false),
                    WatermarkText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HeaderText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FooterText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportCardSettings", x => x.ReportCardSettingId);
                    table.ForeignKey(
                        name: "FK_ReportCardSettings_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "BatchId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportCardSettings_BatchId",
                table: "ReportCardSettings",
                column: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportCardSettings");
        }
    }
}
