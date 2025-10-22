using System.Windows;
using System.Windows.Controls;
using AuroraInvoice.Models;
using AuroraInvoice.Services;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Views;

public partial class DashboardPage : Page
{
    private readonly IDashboardService _dashboardService;

    public DashboardPage()
    {
        InitializeComponent();
        _dashboardService = new DashboardService();
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
            // Load metrics using optimized service (2 queries instead of 5)
            var metrics = await _dashboardService.GetMonthlyMetricsAsync();
            var recentInvoices = await _dashboardService.GetRecentInvoicesAsync(AppConstants.DashboardRecentInvoicesCount);

            // Update UI
            TotalIncomeText.Text = metrics.TotalIncome.ToString("C");
            TotalExpensesText.Text = metrics.TotalExpenses.ToString("C");
            NetGstText.Text = metrics.NetGst.ToString("C");
            PendingInvoicesText.Text = metrics.PendingInvoices.ToString();
            RecentInvoicesGrid.ItemsSource = recentInvoices;

            // Update subtitle based on net GST
            GstSubtitleText.Text = metrics.NetGst >= 0 ? "Payable to ATO" : "Refund from ATO";
        }
        catch (Exception ex)
        {
            await LoggingService.LogErrorAsync(ex, "DashboardPage.LoadDashboardDataAsync");
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
