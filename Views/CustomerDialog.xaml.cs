using System.Windows;
using AuroraInvoice.Data;
using AuroraInvoice.Models;

namespace AuroraInvoice.Views;

public partial class CustomerDialog : Window
{
    private readonly IDbContextFactory<AuroraDbContext> _contextFactory;
    private Customer? _customer;
    private bool _isEditMode;

    public CustomerDialog(IDbContextFactory<AuroraDbContext> contextFactory)
    {
        InitializeComponent();
        _contextFactory = contextFactory;
        _isEditMode = false;
        HeaderText.Text = "New Customer";
    }

    public CustomerDialog(Customer customer, IDbContextFactory<AuroraDbContext> contextFactory)
    {
        InitializeComponent();
        _contextFactory = contextFactory;
        _customer = customer;
        _isEditMode = true;
        HeaderText.Text = "Edit Customer";

        // Populate fields
        NameTextBox.Text = customer.Name;
        ContactPersonTextBox.Text = customer.ContactPerson;
        EmailTextBox.Text = customer.Email;
        PhoneTextBox.Text = customer.Phone;
        ABNTextBox.Text = customer.ABN;
        AddressTextBox.Text = customer.Address;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Please enter a company name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            NameTextBox.Focus();
            return;
        }

        try
        {
            using var context = _contextFactory.CreateDbContext();

            if (_isEditMode && _customer != null)
            {
                // Update existing customer
                var customerToUpdate = await context.Customers.FindAsync(_customer.Id);
                if (customerToUpdate != null)
                {
                    customerToUpdate.Name = NameTextBox.Text.Trim();
                    customerToUpdate.ContactPerson = ContactPersonTextBox.Text.Trim();
                    customerToUpdate.Email = EmailTextBox.Text.Trim();
                    customerToUpdate.Phone = PhoneTextBox.Text.Trim();
                    customerToUpdate.ABN = ABNTextBox.Text.Trim();
                    customerToUpdate.Address = AddressTextBox.Text.Trim();
                    customerToUpdate.ModifiedDate = DateTimeProvider.UtcNow;
                }
            }
            else
            {
                // Create new customer
                var newCustomer = new Customer
                {
                    Name = NameTextBox.Text.Trim(),
                    ContactPerson = ContactPersonTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim(),
                    ABN = ABNTextBox.Text.Trim(),
                    Address = AddressTextBox.Text.Trim(),
                    CreatedDate = DateTimeProvider.UtcNow
                };

                context.Customers.Add(newCustomer);
            }

            await context.SaveChangesAsync();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving customer: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
