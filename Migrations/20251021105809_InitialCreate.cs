using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuroraInvoice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ABN = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BusinessAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    InvoicePrefix = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    NextInvoiceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultPaymentTermsDays = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultGSTRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    BackupFolderPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AutoBackupEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoBackupIntervalDays = table.Column<int>(type: "INTEGER", nullable: false),
                    LastBackupDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ThemeColor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AccentColor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ABN = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiptPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GSTRate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "ABN", "AccentColor", "AutoBackupEnabled", "AutoBackupIntervalDays", "BackupFolderPath", "BusinessAddress", "BusinessName", "CreatedDate", "DefaultGSTRate", "DefaultPaymentTermsDays", "Email", "InvoicePrefix", "LastBackupDate", "LogoPath", "ModifiedDate", "NextInvoiceNumber", "Phone", "ThemeColor" },
                values: new object[] { 1, null, "#7c3aed", false, 7, null, null, "My Business", new DateTime(2025, 10, 21, 21, 58, 7, 877, DateTimeKind.Local).AddTicks(2043), 0.10m, 30, null, "INV-", null, null, null, 1, null, "#2563eb" });

            migrationBuilder.InsertData(
                table: "ExpenseCategories",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 21, 21, 58, 7, 875, DateTimeKind.Local).AddTicks(2295), "Stationery, printer supplies, etc.", true, "Office Supplies" },
                    { 2, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7613), "Office or workspace rental", true, "Rent" },
                    { 3, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7633), "Electricity, water, internet, phone", true, "Utilities" },
                    { 4, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7635), "Transportation, accommodation, meals", true, "Travel" },
                    { 5, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7637), "Cloud services, software licenses", true, "Software Subscriptions" },
                    { 6, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7639), "Legal, accounting, consulting", true, "Professional Services" },
                    { 7, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7640), "Promotional materials, online ads", true, "Marketing & Advertising" },
                    { 8, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7641), "Computers, furniture, machinery", true, "Equipment" },
                    { 9, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7643), "Business insurance premiums", true, "Insurance" },
                    { 10, new DateTime(2025, 10, 21, 21, 58, 7, 876, DateTimeKind.Local).AddTicks(7644), "Miscellaneous expenses", true, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
