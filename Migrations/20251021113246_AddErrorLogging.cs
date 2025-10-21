using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraInvoice.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "TEXT", nullable: true),
                    UserAction = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");
        }
    }
}
