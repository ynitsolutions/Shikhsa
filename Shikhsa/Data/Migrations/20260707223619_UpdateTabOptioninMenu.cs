using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTabOptioninMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TabId",
                table: "RoleMenus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HostelFeePlans",
                columns: table => new
                {
                    HostelFeePlanId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeHeadingId = table.Column<long>(type: "bigint", nullable: false),
                    HostelId = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MealPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HostelFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Batch = table.Column<int>(type: "int", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelFeePlans", x => x.HostelFeePlanId);
                    table.ForeignKey(
                        name: "FK_HostelFeePlans_FeeHeadings_FeeHeadingId",
                        column: x => x.FeeHeadingId,
                        principalTable: "FeeHeadings",
                        principalColumn: "FeeHeadingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuPermissionItem",
                columns: table => new
                {
                    PermissionItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubMenuId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ControllerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuPermissionItem", x => x.PermissionItemId);
                    table.ForeignKey(
                        name: "FK_MenuPermissionItem_Menus_SubMenuId",
                        column: x => x.SubMenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuTab",
                columns: table => new
                {
                    TabId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubMenuId = table.Column<int>(type: "int", nullable: false),
                    TabName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuTab", x => x.TabId);
                    table.ForeignKey(
                        name: "FK_MenuTab_Menus_SubMenuId",
                        column: x => x.SubMenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransportFeePlans",
                columns: table => new
                {
                    TransportFeePlanId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeHeadingId = table.Column<long>(type: "bigint", nullable: false),
                    TransportId = table.Column<int>(type: "int", nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransportFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransportOption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Batch = table.Column<int>(type: "int", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportFeePlans", x => x.TransportFeePlanId);
                    table.ForeignKey(
                        name: "FK_TransportFeePlans_FeeHeadings_FeeHeadingId",
                        column: x => x.FeeHeadingId,
                        principalTable: "FeeHeadings",
                        principalColumn: "FeeHeadingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TuitionFeePlans",
                columns: table => new
                {
                    TuitionFeePlanId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeHeadingId = table.Column<long>(type: "bigint", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    Medium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeeValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Batch = table.Column<int>(type: "int", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuitionFeePlans", x => x.TuitionFeePlanId);
                    table.ForeignKey(
                        name: "FK_TuitionFeePlans_FeeHeadings_FeeHeadingId",
                        column: x => x.FeeHeadingId,
                        principalTable: "FeeHeadings",
                        principalColumn: "FeeHeadingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenus_TabId",
                table: "RoleMenus",
                column: "TabId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelFeePlans_FeeHeadingId",
                table: "HostelFeePlans",
                column: "FeeHeadingId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuPermissionItem_SubMenuId",
                table: "MenuPermissionItem",
                column: "SubMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuTab_SubMenuId",
                table: "MenuTab",
                column: "SubMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportFeePlans_FeeHeadingId",
                table: "TransportFeePlans",
                column: "FeeHeadingId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionFeePlans_FeeHeadingId",
                table: "TuitionFeePlans",
                column: "FeeHeadingId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenus_MenuTab_TabId",
                table: "RoleMenus",
                column: "TabId",
                principalTable: "MenuTab",
                principalColumn: "TabId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenus_MenuTab_TabId",
                table: "RoleMenus");

            migrationBuilder.DropTable(
                name: "HostelFeePlans");

            migrationBuilder.DropTable(
                name: "MenuPermissionItem");

            migrationBuilder.DropTable(
                name: "MenuTab");

            migrationBuilder.DropTable(
                name: "TransportFeePlans");

            migrationBuilder.DropTable(
                name: "TuitionFeePlans");

            migrationBuilder.DropIndex(
                name: "IX_RoleMenus_TabId",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "TabId",
                table: "RoleMenus");
        }
    }
}
