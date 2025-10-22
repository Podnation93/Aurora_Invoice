using AuroraInvoice.Models;

namespace AuroraInvoice.Services.Interfaces;

/// <summary>
/// Service interface for invoice operations
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Get all invoices with customer information
    /// </summary>
    Task<List<Invoice>> GetAllInvoicesAsync();

    /// <summary>
    /// Get invoices with pagination
    /// </summary>
    Task<(List<Invoice> Invoices, int TotalCount)> GetInvoicesAsync(int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Search invoices by customer name or invoice number
    /// </summary>
    Task<List<Invoice>> SearchInvoicesAsync(string searchTerm, InvoiceStatus? status = null);

    /// <summary>
    /// Get invoice by ID with full details (customer, items)
    /// </summary>
    Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);

    /// <summary>
    /// Get invoice summary statistics
    /// </summary>
    Task<InvoiceSummary> GetInvoiceSummaryAsync();

    /// <summary>
    /// Create a new invoice
    /// </summary>
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);

    /// <summary>
    /// Update an existing invoice
    /// </summary>
    Task<Invoice> UpdateInvoiceAsync(Invoice invoice);

    /// <summary>
    /// Delete an invoice
    /// </summary>
    Task<bool> DeleteInvoiceAsync(int invoiceId);

    /// <summary>
    /// Get next invoice number
    /// </summary>
    Task<string> GetNextInvoiceNumberAsync();

    /// <summary>
    /// Mark invoice as sent
    /// </summary>
    Task<bool> MarkInvoiceAsSentAsync(int invoiceId);

    /// <summary>
    /// Mark invoice as paid
    /// </summary>
    Task<bool> MarkInvoiceAsPaidAsync(int invoiceId, DateTime? paymentDate = null);

    /// <summary>
    /// Update overdue invoices (invoices past due date with Sent status)
    /// </summary>
    Task<int> UpdateOverdueInvoicesAsync();
}

/// <summary>
/// Invoice summary statistics DTO
/// </summary>
public class InvoiceSummary
{
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public int TotalCount { get; set; }
    public int DraftCount { get; set; }
    public int SentCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
}
