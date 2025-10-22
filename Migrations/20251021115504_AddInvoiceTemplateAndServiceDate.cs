using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraInvoice.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTemplateAndServiceDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceDate",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShowBusinessLogo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowBusinessAddress = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowBusinessPhone = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowBusinessEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowABN = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowInvoiceNumber = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowInvoiceDate = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowDueDate = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowServiceDate = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowCustomerName = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowCustomerContactPerson = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowCustomerAddress = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowCustomerPhone = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowCustomerEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowCustomerABN = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemNumber = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemDescription = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemServiceDate = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemQuantity = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemUnitPrice = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemGST = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowItemTotal = table.Column<bool>(type: "INTEGER", nullable: false),
                    ItemNumberLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemDescriptionLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemServiceDateLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemQuantityLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemUnitPriceLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemTotalLabel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ShowSubtotal = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowGSTTotal = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowGrandTotal = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowPaymentTerms = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowNotes = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowThankYouMessage = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustomFooterText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    HeaderColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AccentColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceTemplates", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InvoiceTemplates",
                columns: new[] { "Id", "AccentColor", "CreatedDate", "CustomFooterText", "HeaderColor", "IsDefault", "ItemDescriptionLabel", "ItemNumberLabel", "ItemQuantityLabel", "ItemServiceDateLabel", "ItemTotalLabel", "ItemUnitPriceLabel", "ModifiedDate", "ShowABN", "ShowBusinessAddress", "ShowBusinessEmail", "ShowBusinessLogo", "ShowBusinessPhone", "ShowCustomerABN", "ShowCustomerAddress", "ShowCustomerContactPerson", "ShowCustomerEmail", "ShowCustomerName", "ShowCustomerPhone", "ShowDueDate", "ShowGSTTotal", "ShowGrandTotal", "ShowInvoiceDate", "ShowInvoiceNumber", "ShowItemDescription", "ShowItemGST", "ShowItemNumber", "ShowItemQuantity", "ShowItemServiceDate", "ShowItemTotal", "ShowItemUnitPrice", "ShowNotes", "ShowPaymentTerms", "ShowServiceDate", "ShowSubtotal", "ShowThankYouMessage", "TemplateName" },
                values: new object[] { 1, "#008080", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "#008080", true, "Item & Description", "#", "Hrs", "Service Date", "Amount", "Rate", null, true, true, true, false, true, false, true, false, false, true, false, false, false, true, true, true, true, false, true, true, true, true, true, false, false, false, false, false, "NDIS Service Invoice" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "ServiceDate",
                table: "InvoiceItems");
        }
    }
}
