using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;

namespace AuroraInvoice.Views;

public partial class SettingsPage : Page
{
    private AppSettings? _settings;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += SettingsPage_Loaded;
    }

    private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            using var context = new AuroraDbContext();
            _settings = await context.AppSettings.FirstOrDefaultAsync();

            if (_settings != null)
            {
                BusinessNameTextBox.Text = _settings.BusinessName;
                ABNTextBox.Text = _settings.ABN;
                AddressTextBox.Text = _settings.BusinessAddress;
                PhoneTextBox.Text = _settings.Phone;
                EmailTextBox.Text = _settings.Email;
                InvoicePrefixTextBox.Text = _settings.InvoicePrefix;
                NextInvoiceNumberTextBox.Text = _settings.NextInvoiceNumber.ToString();
                PaymentTermsTextBox.Text = _settings.DefaultPaymentTermsDays.ToString();
                DefaultGSTRateTextBox.Text = (_settings.DefaultGSTRate * 100).ToString("F2");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BusinessNameTextBox.Text))
        {
            MessageBox.Show("Please enter a business name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(NextInvoiceNumberTextBox.Text, out int nextInvoiceNumber) || nextInvoiceNumber < 1)
        {
            MessageBox.Show("Please enter a valid next invoice number.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(PaymentTermsTextBox.Text, out int paymentTerms) || paymentTerms < 0)
        {
            MessageBox.Show("Please enter valid payment terms.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(DefaultGSTRateTextBox.Text, out decimal gstRatePercent) || gstRatePercent < 0 || gstRatePercent > 100)
        {
            MessageBox.Show("Please enter a valid GST rate between 0 and 100.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new AuroraDbContext();

            if (_settings != null)
            {
                var settingsToUpdate = await context.AppSettings.FindAsync(_settings.Id);
                if (settingsToUpdate != null)
                {
                    settingsToUpdate.BusinessName = BusinessNameTextBox.Text.Trim();
                    settingsToUpdate.ABN = ABNTextBox.Text.Trim();
                    settingsToUpdate.BusinessAddress = AddressTextBox.Text.Trim();
                    settingsToUpdate.Phone = PhoneTextBox.Text.Trim();
                    settingsToUpdate.Email = EmailTextBox.Text.Trim();
                    settingsToUpdate.InvoicePrefix = InvoicePrefixTextBox.Text.Trim();
                    settingsToUpdate.NextInvoiceNumber = nextInvoiceNumber;
                    settingsToUpdate.DefaultPaymentTermsDays = paymentTerms;
                    settingsToUpdate.DefaultGSTRate = gstRatePercent / 100;
                    settingsToUpdate.ModifiedDate = DateTime.Now;
                }
            }

            await context.SaveChangesAsync();

            MessageBox.Show("Settings saved successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
