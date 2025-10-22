using System.Windows;
using System.Windows.Controls;

namespace AuroraInvoice.Views;

public partial class ReportsPage : Page
{
    private readonly GstSummaryPage _gstSummaryPage;
    private readonly YearlyFinancialReportPage _yearlyFinancialReportPage;
    private readonly InvoiceActivityReportPage _invoiceActivityReportPage;
    private readonly ExpenseReportPage _expenseReportPage;

    public ReportsPage(GstSummaryPage gstSummaryPage, YearlyFinancialReportPage yearlyFinancialReportPage, InvoiceActivityReportPage invoiceActivityReportPage, ExpenseReportPage expenseReportPage)
    {
        InitializeComponent();
        _gstSummaryPage = gstSummaryPage;
        _yearlyFinancialReportPage = yearlyFinancialReportPage;
        _invoiceActivityReportPage = invoiceActivityReportPage;
        _expenseReportPage = expenseReportPage;
    }

    private void GenerateGSTReport_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(_gstSummaryPage);
    }

    private void GenerateYearlyReport_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(_yearlyFinancialReportPage);
    }

    private void GenerateInvoiceReport_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(_invoiceActivityReportPage);
    }

    private void GenerateExpenseReport_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(_expenseReportPage);
    }

    private void GenerateYearlyReport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Yearly financial report generation coming soon!\n\nThis will show:\n- Total income\n- Total expenses\n- Net profit/loss\n- GST summary", "Feature Coming Soon",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GenerateInvoiceReport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Invoice activity report generation coming soon!\n\nThis will show:\n- All invoices by status\n- Payment trends\n- Outstanding invoices", "Feature Coming Soon",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GenerateExpenseReport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Expense report generation coming soon!\n\nThis will show:\n- Expenses by category\n- Monthly trends\n- Top vendors", "Feature Coming Soon",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
