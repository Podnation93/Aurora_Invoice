using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services;
using AuroraInvoice.Services.Interfaces;

namespace AuroraInvoice.Views;

public partial class InvoicesPage : Page
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICustomerService _customerService;
    private List<Invoice> _allInvoices = new();

    public InvoicesPage()
    {
        InitializeComponent();
        _invoiceService = new InvoiceService();
        _customerService = new CustomerService();
        Loaded += InvoicesPage_Loaded;
    }

    private async void InvoicesPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadInvoicesAsync();
    }

    private async Task LoadInvoicesAsync()
    {
        try
        {
            // Update overdue invoices first
            await _invoiceService.UpdateOverdueInvoicesAsync();

            // Load all invoices
            _allInvoices = await _invoiceService.GetAllInvoicesAsync();
            InvoicesGrid.ItemsSource = _allInvoices;

            // Get summary statistics
            var summary = await _invoiceService.GetInvoiceSummaryAsync();

            // Update summary cards
            TotalInvoicedText.Text = summary.TotalInvoiced.ToString("C");
            TotalPaidText.Text = summary.TotalPaid.ToString("C");
            TotalOutstandingText.Text = summary.TotalOutstanding.ToString("C");
            TotalOverdueText.Text = summary.TotalOverdue.ToString("C");

            // Show/hide empty state
            EmptyState.Visibility = _allInvoices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            InvoicesGrid.Visibility = _allInvoices.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            await LoggingService.LogErrorAsync(ex, "InvoicesPage.LoadInvoicesAsync");
            MessageBox.Show($"Error loading invoices: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterInvoices();
    }

    private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterInvoices();
    }

    private void FilterInvoices()
    {
        if (SearchBox == null || StatusFilterComboBox == null)
            return;

        var searchText = SearchBox.Text?.ToLower() ?? string.Empty;
        var filtered = _allInvoices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered.Where(i =>
                i.InvoiceNumber.ToLower().Contains(searchText) ||
                (i.Customer != null && i.Customer.Name.ToLower().Contains(searchText))
            );
        }

        if (StatusFilterComboBox.SelectedIndex > 0)
        {
            var selectedStatus = StatusFilterComboBox.SelectedIndex switch
            {
                1 => InvoiceStatus.Draft,
                2 => InvoiceStatus.Sent,
                3 => InvoiceStatus.Paid,
                4 => InvoiceStatus.Overdue,
                _ => InvoiceStatus.Draft
            };
            filtered = filtered.Where(i => i.Status == selectedStatus);
        }

        InvoicesGrid.ItemsSource = filtered.ToList();
    }

    private async void NewInvoice_Click(object sender, RoutedEventArgs e)
    {
        var (customers, _) = await _customerService.GetCustomersAsync();

        if (customers.Count == 0)
        {
            MessageBox.Show("Please add at least one customer before creating an invoice.", "No Customers",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new InvoiceDialog(customers);
        if (dialog.ShowDialog() == true)
        {
            await LoadInvoicesAsync();
        }
    }

    private async void EditInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Invoice invoice)
        {
            using var context = new AuroraDbContext();
            var customers = await context.Customers.ToListAsync();

            var dialog = new InvoiceDialog(invoice, customers);
            if (dialog.ShowDialog() == true)
            {
                await LoadInvoicesAsync();
            }
        }
    }

    private async void DeleteInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Invoice invoice)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete invoice {invoice.InvoiceNumber}?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new AuroraDbContext();
                    context.Invoices.Remove(invoice);
                    await context.SaveChangesAsync();

                    MessageBox.Show("Invoice deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadInvoicesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting invoice: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private async void InvoicesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (InvoicesGrid.SelectedItem is Invoice invoice)
        {
            using var context = new AuroraDbContext();
            var customers = await context.Customers.ToListAsync();

            var dialog = new InvoiceDialog(invoice, customers);
            if (dialog.ShowDialog() == true)
            {
                await LoadInvoicesAsync();
            }
        }
    }

    private async void PreviewInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Invoice invoice)
        {
            try
            {
                var pdfService = new Services.InvoicePdfService();
                var pdfBytes = await pdfService.GenerateInvoicePdfBytesAsync(invoice);

                // Save to temp file and open
                var tempPath = Path.Combine(Path.GetTempPath(), $"Invoice_{invoice.InvoiceNumber}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                // Open PDF with default viewer
                var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo(tempPath)
                {
                    UseShellExecute = true
                };
                process.Start();
            }
            catch (Exception ex)
            {
                await Services.LoggingService.LogErrorAsync(ex, "InvoicesPage.PreviewInvoice_Click", "User clicked Preview PDF");
                MessageBox.Show($"Error generating invoice preview: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void DownloadInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Invoice invoice)
        {
            try
            {
                // Ask user where to save
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Invoice_{invoice.InvoiceNumber}",
                    DefaultExt = ".pdf",
                    Filter = "PDF files (*.pdf)|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    var pdfService = new Services.InvoicePdfService();
                    await pdfService.GenerateInvoicePdfAsync(invoice, dialog.FileName);

                    MessageBox.Show($"Invoice saved successfully!\n\nLocation: {dialog.FileName}", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                await Services.LoggingService.LogErrorAsync(ex, "InvoicesPage.DownloadInvoice_Click", "User clicked Download PDF");
                MessageBox.Show($"Error saving invoice: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
