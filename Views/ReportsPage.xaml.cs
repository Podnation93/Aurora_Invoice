using System.Windows;
using System.Windows.Controls;

namespace AuroraInvoice.Views;

public partial class ReportsPage : Page
{
    public ReportsPage()
    {
        InitializeComponent();
    }

    private void GenerateGSTReport_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("GST report generation coming soon!\n\nThis will show:\n- GST collected from invoices\n- GST paid on expenses\n- Net GST position", "Feature Coming Soon",
            MessageBoxButton.OK, MessageBoxImage.Information);
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
