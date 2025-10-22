using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for dashboard operations with optimized queries
/// </summary>
public class DashboardService : IDashboardService
{
    /// <summary>
    /// Gets dashboard metrics for the current month using optimized queries
    /// </summary>
    /// <returns>Dashboard metrics data</returns>
    public async Task<DashboardMetrics> GetMonthlyMetricsAsync()
    {
        using var context = new AuroraDbContext();

        var now = DateTimeProvider.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

        // Optimized Query 1: Get all invoice metrics in a single query
        var invoiceMetrics = await context.Invoices
            .Where(i => i.InvoiceDate >= firstDayOfMonth && i.InvoiceDate <= lastDayOfMonth)
            .GroupBy(i => 1)
            .Select(g => new
            {
                TotalIncome = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount),
                GstCollected = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.GSTAmount),
                PendingCount = g.Count(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue)
            })
            .FirstOrDefaultAsync();

        // Optimized Query 2: Get all expense metrics in a single query
        var expenseMetrics = await context.Expenses
            .Where(e => e.Date >= firstDayOfMonth && e.Date <= lastDayOfMonth)
            .GroupBy(e => 1)
            .Select(g => new
            {
                TotalExpenses = g.Sum(e => e.Amount),
                GstPaid = g.Sum(e => e.GSTAmount)
            })
            .FirstOrDefaultAsync();

        // Build metrics object
        var totalIncome = invoiceMetrics?.TotalIncome ?? 0;
        var gstCollected = invoiceMetrics?.GstCollected ?? 0;
        var pendingInvoices = invoiceMetrics?.PendingCount ?? 0;
        var totalExpenses = expenseMetrics?.TotalExpenses ?? 0;
        var gstPaid = expenseMetrics?.GstPaid ?? 0;

        return new DashboardMetrics
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetGst = gstCollected - gstPaid,
            PendingInvoices = pendingInvoices,
            GstCollected = gstCollected,
            GstPaid = gstPaid,
            FirstDayOfMonth = firstDayOfMonth,
            LastDayOfMonth = lastDayOfMonth
        };
    }

    /// <summary>
    /// Gets recent invoices for display on dashboard
    /// </summary>
    /// <param name="count">Number of recent invoices to retrieve</param>
    /// <returns>List of recent invoices with customer information</returns>
    public async Task<List<Invoice>> GetRecentInvoicesAsync(int count = 10)
    {
        using var context = new AuroraDbContext();

        return await context.Invoices
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(count)
            .ToListAsync();
    }
}
