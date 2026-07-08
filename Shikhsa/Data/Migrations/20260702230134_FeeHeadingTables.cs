using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class FeeHeadingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeeFrequencies",
                columns: table => new
                {
                    FrequencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeFrequencies", x => x.FrequencyId);
                });

            migrationBuilder.CreateTable(
                name: "FeeHeadings",
                columns: table => new
                {
                    FeeHeadingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeHeadingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FrequencyId = table.Column<int>(type: "int", nullable: false),
                    Jan = table.Column<bool>(type: "bit", nullable: false),
                    Feb = table.Column<bool>(type: "bit", nullable: false),
                    Mar = table.Column<bool>(type: "bit", nullable: false),
                    Apr = table.Column<bool>(type: "bit", nullable: false),
                    May = table.Column<bool>(type: "bit", nullable: false),
                    Jun = table.Column<bool>(type: "bit", nullable: false),
                    Jul = table.Column<bool>(type: "bit", nullable: false),
                    Aug = table.Column<bool>(type: "bit", nullable: false),
                    Sep = table.Column<bool>(type: "bit", nullable: false),
                    Oct = table.Column<bool>(type: "bit", nullable: false),
                    Nov = table.Column<bool>(type: "bit", nullable: false),
                    Dec = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeHeadings", x => x.FeeHeadingId);
                    table.ForeignKey(
                        name: "FK_FeeHeadings_FeeFrequencies_FrequencyId",
                        column: x => x.FrequencyId,
                        principalTable: "FeeFrequencies",
                        principalColumn: "FrequencyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FeeFrequencies",
                columns: new[] { "FrequencyId", "AddedBy", "AddedDate", "DisplayOrder", "IsActive", "Text", "UpdatedBy", "UpdatedDate", "Value" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, "One Time", null, null, "ONETIME" },
                    { 2, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, true, "Monthly", null, null, "MONTHLY" },
                    { 3, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, true, "Quarterly", null, null, "QUARTERLY" },
                    { 4, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, true, "Half Yearly", null, null, "HALFYEARLY" },
                    { 5, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, true, "Yearly", null, null, "YEARLY" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeeHeadings_FrequencyId",
                table: "FeeHeadings",
                column: "FrequencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeHeadings");

            migrationBuilder.DropTable(
                name: "FeeFrequencies");
        }
    }
}
