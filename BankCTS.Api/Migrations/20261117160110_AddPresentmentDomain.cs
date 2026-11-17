using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankCTS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPresentmentDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChequeInstruments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssuingBankCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IssuingBranchCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    ChequeDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DrawerName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PayeeName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChequeInstruments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChequePresentments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PresentingBankCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PreviousPresentmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChequePresentments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChequePresentments_ChequeInstruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "ChequeInstruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChequePresentments_ChequePresentments_PreviousPresentmentId",
                        column: x => x.PreviousPresentmentId,
                        principalTable: "ChequePresentments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresentmentAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PresentmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreviousStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    NewStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresentmentAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresentmentAuditEvents_ChequePresentments_PresentmentId",
                        column: x => x.PresentmentId,
                        principalTable: "ChequePresentments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChequePresentments_InstrumentId",
                table: "ChequePresentments",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChequePresentments_PreviousPresentmentId",
                table: "ChequePresentments",
                column: "PreviousPresentmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PresentmentAuditEvents_PresentmentId",
                table: "PresentmentAuditEvents",
                column: "PresentmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresentmentAuditEvents");

            migrationBuilder.DropTable(
                name: "ChequePresentments");

            migrationBuilder.DropTable(
                name: "ChequeInstruments");
        }
    }
}
