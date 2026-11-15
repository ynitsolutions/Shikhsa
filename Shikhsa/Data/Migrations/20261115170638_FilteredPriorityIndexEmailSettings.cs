using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FilteredPriorityIndexEmailSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolEmailSetting_SchoolMasters_SchoolId",
                table: "SchoolEmailSetting");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SchoolEmailSetting",
                table: "SchoolEmailSetting");

            migrationBuilder.DropIndex(
                name: "IX_SchoolEmailSetting_SchoolId",
                table: "SchoolEmailSetting");

            migrationBuilder.RenameTable(
                name: "SchoolEmailSetting",
                newName: "SchoolEmailSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SchoolEmailSettings",
                table: "SchoolEmailSettings",
                column: "EmailSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEmailSettings_SchoolId_Priority",
                table: "SchoolEmailSettings",
                columns: new[] { "SchoolId", "Priority" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolEmailSettings_SchoolMasters_SchoolId",
                table: "SchoolEmailSettings",
                column: "SchoolId",
                principalTable: "SchoolMasters",
                principalColumn: "SchoolId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolEmailSettings_SchoolMasters_SchoolId",
                table: "SchoolEmailSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SchoolEmailSettings",
                table: "SchoolEmailSettings");

            migrationBuilder.DropIndex(
                name: "IX_SchoolEmailSettings_SchoolId_Priority",
                table: "SchoolEmailSettings");

            migrationBuilder.RenameTable(
                name: "SchoolEmailSettings",
                newName: "SchoolEmailSetting");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SchoolEmailSetting",
                table: "SchoolEmailSetting",
                column: "EmailSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEmailSetting_SchoolId",
                table: "SchoolEmailSetting",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolEmailSetting_SchoolMasters_SchoolId",
                table: "SchoolEmailSetting",
                column: "SchoolId",
                principalTable: "SchoolMasters",
                principalColumn: "SchoolId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
