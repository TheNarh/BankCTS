using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankCTS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddActivePresentmentUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChequePresentments_InstrumentId",
                table: "ChequePresentments");

            migrationBuilder.CreateIndex(
                name: "IX_ChequePresentments_InstrumentId",
                table: "ChequePresentments",
                column: "InstrumentId",
                unique: true,
                filter: "\"Status\" IN ('Received', 'Validating', 'Pending', 'Submitted', 'Failed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChequePresentments_InstrumentId",
                table: "ChequePresentments");

            migrationBuilder.CreateIndex(
                name: "IX_ChequePresentments_InstrumentId",
                table: "ChequePresentments",
                column: "InstrumentId");
        }
    }
}
