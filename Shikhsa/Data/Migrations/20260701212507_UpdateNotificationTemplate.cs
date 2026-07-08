using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationTemplates_NotificationCategories_NotificationCategoryId",
                table: "NotificationTemplates");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplates_NotificationCategoryId",
                table: "NotificationTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_NotificationCategoryId",
                table: "NotificationTemplates",
                column: "NotificationCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationTemplates_NotificationCategories_NotificationCategoryId",
                table: "NotificationTemplates",
                column: "NotificationCategoryId",
                principalTable: "NotificationCategories",
                principalColumn: "NotificationCategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
