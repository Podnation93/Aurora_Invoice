namespace AuroraInvoice.Services.Interfaces;

/// <summary>
/// Service interface for dashboard operations and metrics
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets dashboard metrics for the current month
    /// </summary>
    /// <returns>Dashboard metrics data</returns>
    Task<DashboardMetrics> GetMonthlyMetricsAsync();

    /// <summary>
    /// Gets recent invoices for display on dashboard
    /// </summary>
    /// <param name="count">Number of recent invoices to retrieve</param>
    /// <returns>List of recent invoices</returns>
    Task<List<Models.Invoice>> GetRecentInvoicesAsync(int count = 10);
}

/// <summary>
/// Dashboard metrics data transfer object
/// </summary>
public class DashboardMetrics
{
    /// <summary>
    /// Total income from paid invoices this month
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Total expenses this month
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// Net GST (collected from invoices minus paid on expenses)
    /// </summary>
    public decimal NetGst { get; set; }

    /// <summary>
    /// Number of pending invoices (Sent or Overdue status)
    /// </summary>
    public int PendingInvoices { get; set; }

    /// <summary>
    /// GST collected from paid invoices
    /// </summary>
    public decimal GstCollected { get; set; }

    /// <summary>
    /// GST paid on expenses
    /// </summary>
    public decimal GstPaid { get; set; }

    /// <summary>
    /// First day of current month (for reference)
    /// </summary>
    public DateTime FirstDayOfMonth { get; set; }

    /// <summary>
    /// Last day of current month (for reference)
    /// </summary>
    public DateTime LastDayOfMonth { get; set; }
}
