using System.Threading.Tasks;
using AuroraInvoice.Data;
using AuroraInvoice.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AuroraInvoice.Services;

public class ReportService : IReportService
{
    private readonly IDbContextFactory<AuroraDbContext> _contextFactory;

    public ReportService(IDbContextFactory<AuroraDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        public async Task<YearlyFinancialSummary> GetYearlyFinancialSummaryAsync(int year)
        {
            using var context = _contextFactory.CreateDbContext();
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
    
            var totalIncome = await context.Invoices
                .Where(i => i.Status == Models.InvoiceStatus.Paid && i.InvoiceDate.Year == year)
                .SumAsync(i => i.TotalAmount);
    
            var totalExpenses = await context.Expenses
                .Where(e => e.Date.Year == year)
                .SumAsync(e => e.Amount);
    
            var totalGstCollected = await context.Invoices
                .Where(i => i.Status == Models.InvoiceStatus.Paid && i.InvoiceDate.Year == year)
                .SumAsync(i => i.GSTAmount);
    
            var totalGstPaid = await context.Expenses
                .Where(e => e.Date.Year == year)
                .SumAsync(e => e.GSTAmount);
    
            return new YearlyFinancialSummary
            {
                Year = year,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                TotalGstCollected = totalGstCollected,
                TotalGstPaid = totalGstPaid
                public async Task<InvoiceActivityReport> GetInvoiceActivityReportAsync(DateTime startDate, DateTime endDate)
                {
                    using var context = _contextFactory.CreateDbContext();
            
                    var invoices = await context.Invoices
                        .Include(i => i.Customer)
                        .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                        .OrderByDescending(i => i.InvoiceDate)
                        .ToListAsync();
            
                    var report = new InvoiceActivityReport
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalInvoices = invoices.Count,
                        TotalInvoiced = invoices.Sum(i => i.TotalAmount),
                        TotalPaid = invoices.Where(i => i.Status == Models.InvoiceStatus.Paid).Sum(i => i.TotalAmount),
                        TotalOutstanding = invoices.Where(i => i.Status == Models.InvoiceStatus.Sent || i.Status == Models.InvoiceStatus.Overdue).Sum(i => i.TotalAmount),
                        Activities = invoices.Select(i => new InvoiceActivity
                        {
                            InvoiceId = i.Id,
                            InvoiceNumber = i.InvoiceNumber,
                            CustomerName = i.Customer.Name,
                            InvoiceDate = i.InvoiceDate,
                            DueDate = i.DueDate,
                            Amount = i.TotalAmount,
                            Status = i.Status
                            public async Task<ExpenseReport> GetExpenseReportAsync(DateTime startDate, DateTime endDate)
                            {
                                using var context = _contextFactory.CreateDbContext();
                        
                                var expenses = await context.Expenses
                                    .Include(e => e.Category)
                                    .Where(e => e.Date >= startDate && e.Date <= endDate)
                                    .OrderByDescending(e => e.Date)
                                    .ToListAsync();
                        
                                var report = new ExpenseReport
                                {
                                    StartDate = startDate,
                                    EndDate = endDate,
                                    TotalExpenses = expenses.Count,
                                    TotalAmount = expenses.Sum(e => e.Amount),
                                    Expenses = expenses.Select(e => new ExpenseReportItem
                                    {
                                        ExpenseId = e.Id,
                                        Date = e.Date,
                                        Vendor = e.Vendor,
                                        Description = e.Description,
                                        Category = e.Category.Name,
                                        Amount = e.Amount
                                    }).ToList()
                                };
                        
                                return report;
                            }
                        }).ToList()
                    };
            
                    return report;
                }
            };
        }
    }

    public async Task<GstSummary> GetGstSummaryAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.CreateDbContext();

        var gstCollected = await context.Invoices
            .Where(i => i.Status == Models.InvoiceStatus.Paid && i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .SumAsync(i => i.GSTAmount);

        var gstPaid = await context.Expenses
            .Where(e => e.Date >= startDate && e.Date <= endDate)
            .SumAsync(e => e.GSTAmount);

        return new GstSummary
        {
            GstCollected = gstCollected,
            GstPaid = gstPaid,
            StartDate = startDate,
            EndDate = endDate
        };
    }
}
