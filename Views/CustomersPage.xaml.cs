using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;

namespace AuroraInvoice.Views;

public partial class CustomersPage : Page
{
    private List<Customer> _allCustomers = new();

    public CustomersPage()
    {
        InitializeComponent();
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
            using var context = new AuroraDbContext();
            _allCustomers = await context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();

            CustomersGrid.ItemsSource = _allCustomers;

            // Show/hide empty state
            EmptyState.Visibility = _allCustomers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CustomersGrid.Visibility = _allCustomers.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            CustomersGrid.ItemsSource = _allCustomers;
        }
        else
        {
            var filtered = _allCustomers.Where(c =>
                c.Name.ToLower().Contains(searchText) ||
                (c.ContactPerson?.ToLower().Contains(searchText) ?? false) ||
                (c.Email?.ToLower().Contains(searchText) ?? false) ||
                (c.ABN?.ToLower().Contains(searchText) ?? false)
            ).ToList();

            CustomersGrid.ItemsSource = filtered;
        }
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
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{customer.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new AuroraDbContext();
                    context.Customers.Remove(customer);
                    await context.SaveChangesAsync();

                    MessageBox.Show("Customer deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
