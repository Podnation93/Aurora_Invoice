using System.Threading.Tasks;
using AuroraInvoice.Models;
using System;

namespace AuroraInvoice.Services.Interfaces;

public interface IReportService
{
    Task<GstSummary> GetGstSummaryAsync(DateTime startDate, DateTime endDate);
    Task<YearlyFinancialSummary> GetYearlyFinancialSummaryAsync(int year);
    Task<InvoiceActivityReport> GetInvoiceActivityReportAsync(DateTime startDate, DateTime endDate);
    Task<ExpenseReport> GetExpenseReportAsync(DateTime startDate, DateTime endDate);
}

public class GstSummary
{
    public decimal GstCollected { get; set; }
    public decimal GstPaid { get; set; }
    public decimal NetGst => GstCollected - GstPaid;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class YearlyFinancialSummary
{
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalIncome - TotalExpenses;
    public decimal TotalGstCollected { get; set; }
    public decimal TotalGstPaid { get; set; }
    public decimal NetGst => TotalGstCollected - TotalGstPaid;
}

public class InvoiceActivityReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public List<InvoiceActivity> Activities { get; set; } = new();
}

public class InvoiceActivity
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public Models.InvoiceStatus Status { get; set; }
}

public class ExpenseReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalExpenses { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ExpenseReportItem> Expenses { get; set; } = new();
}

public class ExpenseReportItem
{
    public int ExpenseId { get; set; }
    public DateTime Date { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
