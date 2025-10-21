using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;

namespace AuroraInvoice.Views;

public partial class InvoicesPage : Page
{
    private List<Invoice> _allInvoices = new();

    public InvoicesPage()
    {
        InitializeComponent();
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
            using var context = new AuroraDbContext();

            _allInvoices = await context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            InvoicesGrid.ItemsSource = _allInvoices;

            // Update summary cards
            TotalInvoicedText.Text = _allInvoices.Sum(i => i.TotalAmount).ToString("C");
            TotalPaidText.Text = _allInvoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount).ToString("C");
            TotalOutstandingText.Text = _allInvoices.Where(i => i.Status == InvoiceStatus.Sent).Sum(i => i.TotalAmount).ToString("C");
            TotalOverdueText.Text = _allInvoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount).ToString("C");

            // Show/hide empty state
            EmptyState.Visibility = _allInvoices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            InvoicesGrid.Visibility = _allInvoices.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
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
        using var context = new AuroraDbContext();
        var customers = await context.Customers.ToListAsync();

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
}
