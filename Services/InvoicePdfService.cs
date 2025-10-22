using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AuroraInvoice.Models;
using AuroraInvoice.Data;
using Microsoft.EntityFrameworkCore;

namespace AuroraInvoice.Services;

public class InvoicePdfService
{
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
        container.Row(row =>
        {
            // Left side - Business info
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(settings.BusinessName)
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Grey.Darken4);

                if (!string.IsNullOrWhiteSpace(settings.ABN))
                {
                    column.Item().Text($"ABN: {settings.ABN}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }

                if (!string.IsNullOrWhiteSpace(settings.BusinessAddress))
                {
                    column.Item().PaddingTop(8).Text(settings.BusinessAddress)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }

                if (!string.IsNullOrWhiteSpace(settings.Phone))
                {
                    column.Item().Text(settings.Phone)
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                }

                if (!string.IsNullOrWhiteSpace(settings.Email))
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
                    .FontColor(Colors.Teal.Darken2);

                column.Item().PaddingTop(4).Text($"# {invoice.InvoiceNumber}")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Grey.Darken3);
            });
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice, AppSettings settings, InvoiceTemplate template)
    {
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

                    leftColumn.Item().PaddingTop(8).Text(invoice.Customer?.Name ?? "")
                        .FontSize(12)
                        .Bold();

                    if (!string.IsNullOrWhiteSpace(invoice.Customer?.ContactPerson))
                    {
                        leftColumn.Item().Text(invoice.Customer.ContactPerson)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(invoice.Customer?.Address))
                    {
                        leftColumn.Item().Text(invoice.Customer.Address)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(invoice.Customer?.Email))
                    {
                        leftColumn.Item().Text(invoice.Customer.Email)
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                // Dates
                row.RelativeItem().AlignRight().Column(rightColumn =>
                {
                    rightColumn.Item().Row(dateRow =>
                    {
                        dateRow.AutoItem().Width(120).Text("Invoice Date :")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2);
                        dateRow.AutoItem().Text(invoice.InvoiceDate.ToString("dd MMMM yyyy"))
                            .FontSize(11);
                    });

                    rightColumn.Item().PaddingTop(4).Row(dateRow =>
                    {
                        dateRow.AutoItem().Width(120).Text("Due Date :")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2);
                        dateRow.AutoItem().Text(invoice.DueDate.ToString("dd MMMM yyyy"))
                            .FontSize(11);
                    });
                });
            });

            column.Item().PaddingTop(30);

            // Invoice Items Table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // #
                    columns.RelativeColumn(4);    // Description
                    columns.ConstantColumn(80);   // Qty
                    columns.ConstantColumn(100);  // Rate
                    columns.ConstantColumn(100);  // Amount
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Teal.Darken2)
                        .Padding(8)
                        .Text("#")
                        .FontColor(Colors.White)
                        .Bold();

                    header.Cell().Background(Colors.Teal.Darken2)
                        .Padding(8)
                        .Text("Item & Description")
                        .FontColor(Colors.White)
                        .Bold();

                    header.Cell().Background(Colors.Teal.Darken2)
                        .Padding(8)
                        .AlignRight()
                        .Text("Qty")
                        .FontColor(Colors.White)
                        .Bold();

                    header.Cell().Background(Colors.Teal.Darken2)
                        .Padding(8)
                        .AlignRight()
                        .Text("Rate")
                        .FontColor(Colors.White)
                        .Bold();

                    header.Cell().Background(Colors.Teal.Darken2)
                        .Padding(8)
                        .AlignRight()
                        .Text("Amount")
                        .FontColor(Colors.White)
                        .Bold();
                });

                // Rows
                int rowNumber = 1;
                foreach (var item in invoice.InvoiceItems)
                {
                    var backgroundColor = rowNumber % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Background(backgroundColor)
                        .Padding(8)
                        .Text(rowNumber.ToString());

                    table.Cell().Background(backgroundColor)
                        .Padding(8)
                        .Text(item.Description);

                    table.Cell().Background(backgroundColor)
                        .Padding(8)
                        .AlignRight()
                        .Text(item.Quantity.ToString("F2"));

                    table.Cell().Background(backgroundColor)
                        .Padding(8)
                        .AlignRight()
                        .Text(item.UnitPrice.ToString("C"));

                    table.Cell().Background(backgroundColor)
                        .Padding(8)
                        .AlignRight()
                        .Text((item.LineTotal + item.GSTAmount).ToString("C"));

                    rowNumber++;
                }
            });

            column.Item().PaddingTop(20);

            // Totals section
            column.Item().AlignRight().Width(250).Column(totalsColumn =>
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

                totalsColumn.Item().Background(Colors.Teal.Darken2)
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
            });

            // Notes section
            if (!string.IsNullOrWhiteSpace(invoice.Notes))
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
            if (settings.DefaultPaymentTermsDays > 0)
            {
                column.Item().Text($"Payment Terms: {settings.DefaultPaymentTermsDays} days")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            }

            column.Item().PaddingTop(4).Text("Thank you for your business!")
                .FontSize(9)
                .Italic()
                .FontColor(Colors.Grey.Darken1);
        });
    }
}
