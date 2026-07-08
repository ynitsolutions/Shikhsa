using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUrlAddActionNameInMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================
            // ADD NEW COLUMNS
            // =========================================

            migrationBuilder.AddColumn<string>(
                name: "ControllerName",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionName",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: true);

            // =========================================
            // UPDATE EXISTING DATA
            // =========================================

            migrationBuilder.Sql(@"
        UPDATE Menus
        SET
            ControllerName =
                PARSENAME(
                    REPLACE(
                        SUBSTRING(Url, 2, LEN(Url)),
                        '/',
                        '.'
                    ),
                    2
                ),

            ActionName =
                PARSENAME(
                    REPLACE(
                        SUBSTRING(Url, 2, LEN(Url)),
                        '/',
                        '.'
                    ),
                    1
                )
        WHERE Url IS NOT NULL
    ");

            // =========================================
            // DROP OLD URL COLUMN
            // =========================================

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Menus");

            // =========================================
            // CREATE DATALISTS
            // =========================================

            migrationBuilder.CreateTable(
                name: "DataLists",
                columns: table => new
                {
                    DataListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    DataListName = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),

                    Description = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    AddedDate = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    UpdatedDate = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true),

                    AddedBy = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    UpdatedBy = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_DataLists",
                        x => x.DataListId);
                });

            // =========================================
            // CREATE DATALIST ITEMS
            // =========================================

            migrationBuilder.CreateTable(
                name: "DataListItems",
                columns: table => new
                {
                    DataListItemId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    DataListId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    DataListItemText = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),

                    DataListItemValue = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    DisplayOrder = table.Column<int>(
                        type: "int",
                        nullable: false),

                    AddedDate = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    UpdatedDate = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true),

                    AddedBy = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    UpdatedBy = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_DataListItems",
                        x => x.DataListItemId);

                    table.ForeignKey(
                        name: "FK_DataListItems_DataLists_DataListId",
                        column: x => x.DataListId,
                        principalTable: "DataLists",
                        principalColumn: "DataListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataListItems_DataListId",
                table: "DataListItems",
                column: "DataListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataListItems");

            migrationBuilder.DropTable(
                name: "DataLists");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "ControllerName",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ActionName",
                table: "Menus");
        }
    }
}
