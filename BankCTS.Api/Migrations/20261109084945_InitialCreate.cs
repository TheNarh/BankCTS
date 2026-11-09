using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankCTS.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cheques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChequeNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ChequeDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PresentationDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DrawerName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PayeeName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cheques", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cheques");
        }
    }
}
