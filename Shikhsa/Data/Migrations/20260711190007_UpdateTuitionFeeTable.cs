using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shikhsa.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTuitionFeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Batch",
                table: "TransportFeePlans",
                newName: "BatchId");

            migrationBuilder.RenameColumn(
                name: "Batch",
                table: "HostelFeePlans",
                newName: "BatchId");

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "TuitionFeePlans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuitionFeePlans_BatchId",
                table: "TuitionFeePlans",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportFeePlans_BatchId",
                table: "TransportFeePlans",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelFeePlans_BatchId",
                table: "HostelFeePlans",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_HostelFeePlans_Batches_BatchId",
                table: "HostelFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportFeePlans_Batches_BatchId",
                table: "TransportFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HostelFeePlans_Batches_BatchId",
                table: "HostelFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportFeePlans_Batches_BatchId",
                table: "TransportFeePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionFeePlans_Batches_BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.DropIndex(
                name: "IX_TuitionFeePlans_BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.DropIndex(
                name: "IX_TransportFeePlans_BatchId",
                table: "TransportFeePlans");

            migrationBuilder.DropIndex(
                name: "IX_HostelFeePlans_BatchId",
                table: "HostelFeePlans");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "TuitionFeePlans");

            migrationBuilder.RenameColumn(
                name: "BatchId",
                table: "TransportFeePlans",
                newName: "Batch");

            migrationBuilder.RenameColumn(
                name: "BatchId",
                table: "HostelFeePlans",
                newName: "Batch");
        }
    }
}
