using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AuroraInvoice.Models;
using AuroraInvoice.Data;
using AuroraInvoice.Common;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AuroraInvoice.Services;

public class InvoicePdfService
{
    private readonly AppConfiguration _config;

    public InvoicePdfService()
    {
        _config = AppConfiguration.Instance;
    }
    /// <summary>
    /// Generate a PDF invoice and save to file
    /// </summary>
    public async Task<string> GenerateInvoicePdfAsync(Invoice invoice, string outputPath)
    {
        using var context = new AuroraDbContext();

        // Load full invoice with related data
        var fullInvoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        if (fullInvoice == null)
            throw new Exception("Invoice not found");

        // Load business settings
        var settings = await context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
            throw new Exception("Business settings not configured");

        // Load template (use default if available)
        var template = await context.InvoiceTemplates
            .Where(t => t.IsDefault)
            .FirstOrDefaultAsync();
        if (template == null)
            template = await context.InvoiceTemplates.FirstOrDefaultAsync();

        // Generate PDF
        var document = CreateInvoiceDocument(fullInvoice, settings, template);
        document.GeneratePdf(outputPath);

        return outputPath;
    }

    /// <summary>
    /// Generate invoice PDF and return as byte array for preview
    /// </summary>
    public async Task<byte[]> GenerateInvoicePdfBytesAsync(Invoice invoice)
    {
        using var context = new AuroraDbContext();

        var fullInvoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        if (fullInvoice == null)
            throw new Exception("Invoice not found");

        var settings = await context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
            throw new Exception("Business settings not configured");

        // Load template (use default if available)
        var template = await context.InvoiceTemplates
            .Where(t => t.IsDefault)
            .FirstOrDefaultAsync();
        if (template == null)
            template = await context.InvoiceTemplates.FirstOrDefaultAsync();

        var document = CreateInvoiceDocument(fullInvoice, settings, template);
        return document.GeneratePdf();
    }

    private Document CreateInvoiceDocument(Invoice invoice, AppSettings settings, InvoiceTemplate? template)
    {
        // Use default template if none provided
        template ??= new InvoiceTemplate();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, invoice, settings, template));
                page.Content().Element(c => ComposeContent(c, invoice, settings, template));
                page.Footer().Element(c => ComposeFooter(c, invoice, settings, template));
            });
        });
    }

    private void ComposeHeader(IContainer container, Invoice invoice, AppSettings settings, InvoiceTemplate template)
    {
        var headerColor = ParseColor(template.HeaderColor);

        container.Row(row =>
        {
            // Left side - Business info
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(settings.BusinessName)
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Grey.Darken4);

                if (template.ShowABN && !string.IsNullOrWhiteSpace(settings.ABN))
                {
                    column.Item().Text($"ABN: {settings.ABN}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }

                if (template.ShowBusinessAddress && !string.IsNullOrWhiteSpace(settings.BusinessAddress))
                {
                    column.Item().PaddingTop(8).Text(settings.BusinessAddress)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }

                if (template.ShowBusinessPhone && !string.IsNullOrWhiteSpace(settings.Phone))
                {
                    column.Item().Text(settings.Phone)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }

                if (template.ShowBusinessEmail && !string.IsNullOrWhiteSpace(settings.Email))
                {
                    column.Item().Text(settings.Email)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }
            });

            // Right side - Invoice title and number
            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().Text("Tax Invoice")
                    .FontSize(28)
                    .Bold()
                    .FontColor(headerColor);

                if (template.ShowInvoiceNumber)
                {
                    column.Item().PaddingTop(4).Text($"# {invoice.InvoiceNumber}")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);
                }
            });
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice, AppSettings settings, InvoiceTemplate template)
    {
        var dateFormat = _config.Templates.DateFormat;

        container.PaddingVertical(20).Column(column =>
        {
            // Bill To and Dates section
            column.Item().Row(row =>
            {
                // Bill To
                row.RelativeItem().Column(leftColumn =>
                {
                    leftColumn.Item().Text("Bill To")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    if (template.ShowCustomerName && invoice.Customer != null)
                    {
                        leftColumn.Item().PaddingTop(8).Text(invoice.Customer.Name)
                            .FontSize(12)
                            .Bold();
                    }

                    if (template.ShowCustomerContactPerson && !string.IsNullOrWhiteSpace(invoice.Customer?.ContactPerson))
                    {
                        leftColumn.Item().Text(invoice.Customer.ContactPerson)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (template.ShowCustomerAddress && !string.IsNullOrWhiteSpace(invoice.Customer?.Address))
                    {
                        leftColumn.Item().Text(invoice.Customer.Address)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (template.ShowCustomerPhone && !string.IsNullOrWhiteSpace(invoice.Customer?.Phone))
                    {
                        leftColumn.Item().Text(invoice.Customer.Phone)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (template.ShowCustomerEmail && !string.IsNullOrWhiteSpace(invoice.Customer?.Email))
                    {
                        leftColumn.Item().Text(invoice.Customer.Email)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (template.ShowCustomerABN && !string.IsNullOrWhiteSpace(invoice.Customer?.ABN))
                    {
                        leftColumn.Item().Text($"ABN: {invoice.Customer.ABN}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                // Dates
                row.RelativeItem().AlignRight().Column(rightColumn =>
                {
                    if (template.ShowInvoiceDate)
                    {
                        rightColumn.Item().Row(dateRow =>
                        {
                            dateRow.AutoItem().Width(120).Text("Invoice Date :")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken2);
                            dateRow.AutoItem().Text(invoice.InvoiceDate.ToString(dateFormat))
                                .FontSize(11);
                        });
                    }

                    if (template.ShowDueDate)
                    {
                        rightColumn.Item().PaddingTop(4).Row(dateRow =>
                        {
                            dateRow.AutoItem().Width(120).Text("Due Date :")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken2);
                            dateRow.AutoItem().Text(invoice.DueDate.ToString(dateFormat))
                                .FontSize(11);
                        });
                    }
                });
            });

            column.Item().PaddingTop(30);

            // Invoice Items Table
            var accentColor = ParseColor(template.AccentColor);
            var numberFormat = _config.Templates.NumberFormat;

            column.Item().Table(table =>
            {
                // Dynamic column definition based on template settings
                table.ColumnsDefinition(columns =>
                {
                    if (template.ShowItemNumber) columns.ConstantColumn(40);
                    if (template.ShowItemDescription) columns.RelativeColumn(4);
                    if (template.ShowItemServiceDate) columns.ConstantColumn(100);
                    if (template.ShowItemQuantity) columns.ConstantColumn(80);
                    if (template.ShowItemUnitPrice) columns.ConstantColumn(100);
                    if (template.ShowItemGST) columns.ConstantColumn(80);
                    if (template.ShowItemTotal) columns.ConstantColumn(100);
                });

                // Header
                table.Header(header =>
                {
                    if (template.ShowItemNumber)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .Text(template.ItemNumberLabel)
                            .FontColor(Colors.White)
                            .Bold();
                    }

                    if (template.ShowItemDescription)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .Text(template.ItemDescriptionLabel)
                            .FontColor(Colors.White)
                            .Bold();
                    }

                    if (template.ShowItemServiceDate)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(template.ItemServiceDateLabel)
                            .FontColor(Colors.White)
                            .Bold();
                    }

                    if (template.ShowItemQuantity)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(template.ItemQuantityLabel)
                            .FontColor(Colors.White)
                            .Bold();
                    }

                    if (template.ShowItemUnitPrice)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(template.ItemUnitPriceLabel)
                            .FontColor(Colors.White)
                            .Bold();
                    }

                    if (template.ShowItemGST)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .AlignRight()
                            .Text("GST")
                            .FontColor(Colors.White)
                            .Bold();
                    }

                    if (template.ShowItemTotal)
                    {
                        header.Cell().Background(accentColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(template.ItemTotalLabel)
                            .FontColor(Colors.White)
                            .Bold();
                    }
                });

                // Rows
                int rowNumber = 1;
                foreach (var item in invoice.InvoiceItems)
                {
                    var backgroundColor = rowNumber % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                    if (template.ShowItemNumber)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .Text(rowNumber.ToString());
                    }

                    if (template.ShowItemDescription)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .Text(item.Description);
                    }

                    if (template.ShowItemServiceDate)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(item.ServiceDate?.ToString(dateFormat) ?? "");
                    }

                    if (template.ShowItemQuantity)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(item.Quantity.ToString(numberFormat));
                    }

                    if (template.ShowItemUnitPrice)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(item.UnitPrice.ToString("C"));
                    }

                    if (template.ShowItemGST)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .AlignRight()
                            .Text(item.GSTAmount.ToString("C"));
                    }

                    if (template.ShowItemTotal)
                    {
                        table.Cell().Background(backgroundColor)
                            .Padding(8)
                            .AlignRight()
                            .Text((item.LineTotal + item.GSTAmount).ToString("C"));
                    }

                    rowNumber++;
                }
            });

            column.Item().PaddingTop(20);

            // Totals section
            column.Item().AlignRight().Width(250).Column(totalsColumn =>
            {
                if (template.ShowSubtotal)
                {
                    totalsColumn.Item().Background(Colors.Grey.Lighten4)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal")
                                .FontSize(11);
                            row.AutoItem().Text(invoice.SubTotal.ToString("C"))
                                .FontSize(11)
                                .Bold();
                        });
                }

                if (template.ShowGSTTotal)
                {
                    totalsColumn.Item().Background(Colors.Grey.Lighten4)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Text($"GST ({(settings.DefaultGSTRate * 100):F0}%)")
                                .FontSize(11);
                            row.AutoItem().Text(invoice.GSTAmount.ToString("C"))
                                .FontSize(11)
                                .Bold();
                        });
                }

                if (template.ShowGrandTotal)
                {
                    totalsColumn.Item().Background(accentColor)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Text("Total")
                                .FontSize(13)
                                .Bold()
                                .FontColor(Colors.White);
                            row.AutoItem().Text(invoice.TotalAmount.ToString("C"))
                                .FontSize(13)
                                .Bold()
                                .FontColor(Colors.White);
                        });
                }
            });

            // Notes section
            if (template.ShowNotes && !string.IsNullOrWhiteSpace(invoice.Notes))
            {
                column.Item().PaddingTop(30).Column(notesColumn =>
                {
                    notesColumn.Item().Text("Notes:")
                        .FontSize(11)
                        .Bold()
                        .FontColor(Colors.Grey.Darken2);

                    notesColumn.Item().PaddingTop(4).Text(invoice.Notes)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, Invoice invoice, AppSettings settings, InvoiceTemplate template)
    {
        container.AlignCenter().Column(column =>
        {
            if (template.ShowPaymentTerms && settings.DefaultPaymentTermsDays > 0)
            {
                column.Item().Text($"Payment Terms: {settings.DefaultPaymentTermsDays} days")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            }

            if (template.ShowThankYouMessage)
            {
                column.Item().PaddingTop(4).Text("Thank you for your business!")
                    .FontSize(9)
                    .Italic()
                    .FontColor(Colors.Grey.Darken1);
            }

            if (!string.IsNullOrWhiteSpace(template.CustomFooterText))
            {
                column.Item().PaddingTop(4).Text(template.CustomFooterText)
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            }
        });
    }

    /// <summary>
    /// Parse hex color string to QuestPDF color
    /// </summary>
    private string ParseColor(string hexColor)
    {
        // Remove # if present
        hexColor = hexColor.TrimStart('#');

        // Ensure valid hex color (6 characters)
        if (hexColor.Length != 6)
        {
            return Colors.Teal.Darken2; // Default color
        }

        try
        {
            // Parse RGB values
            int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

            // QuestPDF uses hex color strings
            return $"#{hexColor}";
        }
        catch
        {
            return Colors.Teal.Darken2; // Default color on error
        }
    }
}
