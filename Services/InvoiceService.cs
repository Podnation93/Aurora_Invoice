using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Common;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for invoice operations with business logic and audit logging
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly IAuditService _auditService;

    public InvoiceService()
    {
        _auditService = new AuditService();
    }

    public InvoiceService(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <inheritdoc/>
    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        using var context = new AuroraDbContext();
        return await context.Invoices
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<(List<Invoice> Invoices, int TotalCount)> GetInvoicesAsync(int pageNumber = 1, int pageSize = 50)
    {
        using var context = new AuroraDbContext();

        var totalCount = await context.Invoices.CountAsync();

        var invoices = await context.Invoices
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (invoices, totalCount);
    }

    /// <inheritdoc/>
    public async Task<List<Invoice>> SearchInvoicesAsync(string searchTerm, InvoiceStatus? status = null)
    {
        using var context = new AuroraDbContext();

        var query = context.Invoices
            .Include(i => i.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(lowerSearchTerm) ||
                (i.Customer != null && i.Customer.Name.ToLower().Contains(lowerSearchTerm))
            );
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        return await query
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
    {
        using var context = new AuroraDbContext();
        return await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
    }

    /// <inheritdoc/>
    public async Task<InvoiceSummary> GetInvoiceSummaryAsync()
    {
        using var context = new AuroraDbContext();

        var invoices = await context.Invoices.ToListAsync();

        return new InvoiceSummary
        {
            TotalInvoiced = invoices.Sum(i => i.TotalAmount),
            TotalPaid = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount),
            TotalOutstanding = invoices.Where(i => i.Status == InvoiceStatus.Sent).Sum(i => i.TotalAmount),
            TotalOverdue = invoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount),
            TotalCount = invoices.Count,
            DraftCount = invoices.Count(i => i.Status == InvoiceStatus.Draft),
            SentCount = invoices.Count(i => i.Status == InvoiceStatus.Sent),
            PaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            OverdueCount = invoices.Count(i => i.Status == InvoiceStatus.Overdue)
        };
    }

    /// <inheritdoc/>
    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        using var context = new AuroraDbContext();

        invoice.CreatedDate = DateTimeProvider.UtcNow;
        invoice.ModifiedDate = DateTimeProvider.UtcNow;

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "Create",
            "Invoice",
            invoice.Id,
            $"Created invoice {invoice.InvoiceNumber} for customer {invoice.CustomerId}"
        );

        return invoice;
    }

    /// <inheritdoc/>
    public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
    {
        using var context = new AuroraDbContext();

        var existingInvoice = await context.Invoices.FindAsync(invoice.Id);
        if (existingInvoice == null)
        {
            throw new InvalidOperationException($"Invoice with ID {invoice.Id} not found");
        }

        // Update properties
        existingInvoice.InvoiceNumber = invoice.InvoiceNumber;
        existingInvoice.CustomerId = invoice.CustomerId;
        existingInvoice.InvoiceDate = invoice.InvoiceDate;
        existingInvoice.DueDate = invoice.DueDate;
        existingInvoice.Status = invoice.Status;
        existingInvoice.SubTotal = invoice.SubTotal;
        existingInvoice.GSTAmount = invoice.GSTAmount;
        existingInvoice.TotalAmount = invoice.TotalAmount;
        existingInvoice.Notes = invoice.Notes;
        existingInvoice.ModifiedDate = DateTimeProvider.UtcNow;

        await context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "Update",
            "Invoice",
            invoice.Id,
            $"Updated invoice {invoice.InvoiceNumber}"
        );

        return existingInvoice;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteInvoiceAsync(int invoiceId)
    {
        using var context = new AuroraDbContext();

        var invoice = await context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            return false;
        }

        var invoiceNumber = invoice.InvoiceNumber;

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "Delete",
            "Invoice",
            invoiceId,
            $"Deleted invoice {invoiceNumber}"
        );

        return true;
    }

    /// <inheritdoc/>
    public async Task<string> GetNextInvoiceNumberAsync()
    {
        using var context = new AuroraDbContext();
        var settings = await context.AppSettings.FirstAsync();
        return $"{settings.InvoicePrefix}{settings.NextInvoiceNumber:D4}";
    }

    /// <inheritdoc/>
    public async Task<bool> MarkInvoiceAsSentAsync(int invoiceId)
    {
        using var context = new AuroraDbContext();

        var invoice = await context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = InvoiceStatus.Sent;
        invoice.ModifiedDate = DateTimeProvider.UtcNow;

        await context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "StatusChange",
            "Invoice",
            invoiceId,
            $"Invoice {invoice.InvoiceNumber} marked as Sent"
        );

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> MarkInvoiceAsPaidAsync(int invoiceId, DateTime? paymentDate = null)
    {
        using var context = new AuroraDbContext();

        var invoice = await context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.ModifiedDate = DateTimeProvider.UtcNow;

        await context.SaveChangesAsync();

        await _auditService.LogAuditAsync(
            "StatusChange",
            "Invoice",
            invoiceId,
            $"Invoice {invoice.InvoiceNumber} marked as Paid on {paymentDate?.ToString("yyyy-MM-dd") ?? "today"}"
        );

        return true;
    }

    /// <inheritdoc/>
    public async Task<int> UpdateOverdueInvoicesAsync()
    {
        using var context = new AuroraDbContext();

        var today = DateTimeProvider.UtcNow.Date;

        var overdueInvoices = await context.Invoices
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < today)
            .ToListAsync();

        foreach (var invoice in overdueInvoices)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.ModifiedDate = DateTimeProvider.UtcNow;
        }

        await context.SaveChangesAsync();

        if (overdueInvoices.Count > 0)
        {
            await _auditService.LogAuditAsync(
                "BulkUpdate",
                "Invoice",
                0,
                $"Updated {overdueInvoices.Count} invoices to Overdue status"
            );
        }

        return overdueInvoices.Count;
    }
}
