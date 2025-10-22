using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Views;

public partial class CustomersPage : Page
{
    private readonly ICustomerService _customerService;
    private int _currentPage = 1;
    private readonly int _pageSize = AppConstants.DefaultPageSize;
    private int _totalCount = 0;
    private string _currentSearchText = string.Empty;

    public CustomersPage()
    {
        InitializeComponent();

        // Initialize services
        var auditService = new AuditService();
        _customerService = new CustomerService(auditService);

        Loaded += CustomersPage_Loaded;
    }

    private async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            List<Customer> customers;

            if (string.IsNullOrWhiteSpace(_currentSearchText))
            {
                var result = await _customerService.GetCustomersAsync(_currentPage, _pageSize);
                customers = result.Customers;
                _totalCount = result.TotalCount;
            }
            else
            {
                var result = await _customerService.SearchCustomersAsync(_currentSearchText, _currentPage, _pageSize);
                customers = result.Customers;
                _totalCount = result.TotalCount;
            }

            CustomersGrid.ItemsSource = customers;

            // Show/hide empty state
            EmptyState.Visibility = customers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CustomersGrid.Visibility = customers.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            // Update pagination info if you have UI for it
            // UpdatePaginationInfo();
        }
        catch (Exception ex)
        {
            await LoggingService.LogErrorAsync(ex, "CustomersPage.LoadCustomersAsync");
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _currentSearchText = SearchBox.Text;
        _currentPage = 1; // Reset to first page when searching

        await LoadCustomersAsync();
    }

    private void NewCustomer_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CustomerDialog();
        if (dialog.ShowDialog() == true)
        {
            _ = LoadCustomersAsync();
        }
    }

    private void EditCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Customer customer)
        {
            var dialog = new CustomerDialog(customer);
            if (dialog.ShowDialog() == true)
            {
                _ = LoadCustomersAsync();
            }
        }
    }

    private async void DeleteCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Customer customer)
        {
            try
            {
                // Check if customer has invoices
                var invoiceCount = await _customerService.GetInvoiceCountAsync(customer.Id);

                if (invoiceCount > 0)
                {
                    MessageBox.Show(
                        $"Cannot delete '{customer.Name}' because {invoiceCount} invoice(s) are associated with this customer.\n\n" +
                        "Please delete or reassign the invoices first.",
                        "Cannot Delete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{customer.Name}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _customerService.DeleteCustomerAsync(customer.Id);

                    MessageBox.Show("Customer deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadCustomersAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                // Service already checked for invoices, but catch just in case
                await LoggingService.LogErrorAsync(ex, "CustomersPage.DeleteCustomer_Click");
                MessageBox.Show(ex.Message, "Cannot Delete",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                await LoggingService.LogErrorAsync(ex, "CustomersPage.DeleteCustomer_Click");
                MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Handle selection if needed
    }

    private void CustomersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CustomersGrid.SelectedItem is Customer customer)
        {
            var dialog = new CustomerDialog(customer);
            if (dialog.ShowDialog() == true)
            {
                _ = LoadCustomersAsync();
            }
        }
    }
}
