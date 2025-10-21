using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;

namespace AuroraInvoice.Views;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += DashboardPage_Loaded;
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            using var context = new AuroraDbContext();

            // Get current month date range
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Calculate total income (from paid invoices this month)
            var totalIncome = await context.Invoices
                .Where(i => i.Status == InvoiceStatus.Paid &&
                           i.InvoiceDate >= firstDayOfMonth &&
                           i.InvoiceDate <= lastDayOfMonth)
                .SumAsync(i => i.TotalAmount);

            // Calculate total expenses this month
            var totalExpenses = await context.Expenses
                .Where(e => e.Date >= firstDayOfMonth &&
                           e.Date <= lastDayOfMonth)
                .SumAsync(e => e.Amount);

            // Calculate net GST (collected - paid)
            var gstCollected = await context.Invoices
                .Where(i => i.Status == InvoiceStatus.Paid &&
                           i.InvoiceDate >= firstDayOfMonth &&
                           i.InvoiceDate <= lastDayOfMonth)
                .SumAsync(i => i.GSTAmount);

            var gstPaid = await context.Expenses
                .Where(e => e.Date >= firstDayOfMonth &&
                           e.Date <= lastDayOfMonth)
                .SumAsync(e => e.GSTAmount);

            var netGst = gstCollected - gstPaid;

            // Count pending invoices
            var pendingInvoices = await context.Invoices
                .CountAsync(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue);

            // Load recent invoices
            var recentInvoices = await context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(10)
                .ToListAsync();

            // Update UI
            TotalIncomeText.Text = totalIncome.ToString("C");
            TotalExpensesText.Text = totalExpenses.ToString("C");
            NetGstText.Text = netGst.ToString("C");
            PendingInvoicesText.Text = pendingInvoices.ToString();
            RecentInvoicesGrid.ItemsSource = recentInvoices;

            // Update subtitle based on net GST
            GstSubtitleText.Text = netGst >= 0 ? "Payable to ATO" : "Refund from ATO";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RecentInvoicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO: Navigate to invoice details
    }

    private void NewInvoice_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new InvoicesPage());
    }

    private void NewCustomer_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new CustomersPage());
    }

    private void AddExpense_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ExpensesPage());
    }

    private void ViewReports_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ReportsPage());
    }

    private void BackupData_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new BackupPage());
    }
}
