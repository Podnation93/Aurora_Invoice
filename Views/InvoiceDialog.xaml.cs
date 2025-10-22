using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Views;

public partial class InvoiceDialog : Window
{
    private readonly IDbContextFactory<AuroraDbContext> _contextFactory;
    private readonly ISettingsService _settingsService;
    private Invoice? _invoice;
    private bool _isEditMode;
    private List<Customer> _customers;
    private ObservableCollection<InvoiceItem> _items = new();
    private readonly GstCalculationService _gstService;

    public InvoiceDialog(List<Customer> customers, string nextInvoiceNumber, IDbContextFactory<AuroraDbContext> contextFactory, ISettingsService settingsService)
    {
        InitializeComponent();
        _customers = customers;
        _contextFactory = contextFactory;
        _settingsService = settingsService;
        _gstService = new GstCalculationService(_settingsService);
        _isEditMode = false;
        HeaderText.Text = "New Invoice";

        CustomerComboBox.ItemsSource = _customers;
        if (_customers.Count > 0)
            CustomerComboBox.SelectedIndex = 0;

        ItemsGrid.ItemsSource = _items;

        InvoiceDatePicker.SelectedDate = DateTimeProvider.ToLocalTime(DateTimeProvider.UtcNow);
        DueDatePicker.SelectedDate = DateTimeProvider.ToLocalTime(DateTimeProvider.UtcNow.AddDays(AppConstants.DefaultPaymentTermsDays));
        InvoiceNumberTextBox.Text = nextInvoiceNumber;
    }

    public InvoiceDialog(Invoice invoice, List<Customer> customers, IDbContextFactory<AuroraDbContext> contextFactory, ISettingsService settingsService)
    {
        InitializeComponent();
        _invoice = invoice;
        _customers = customers;
        _contextFactory = contextFactory;
        _settingsService = settingsService;
        _gstService = new GstCalculationService(_settingsService);
        _isEditMode = true;
        HeaderText.Text = "Edit Invoice";

        CustomerComboBox.ItemsSource = _customers;
        ItemsGrid.ItemsSource = _items;

        LoadInvoiceData();
    }


    private async void LoadInvoiceData()
    {
        if (_invoice == null) return;

        using var context = new AuroraDbContext();

        var fullInvoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == _invoice.Id);

        if (fullInvoice == null) return;

        CustomerComboBox.SelectedItem = _customers.FirstOrDefault(c => c.Id == fullInvoice.CustomerId);
        InvoiceNumberTextBox.Text = fullInvoice.InvoiceNumber;
        InvoiceDatePicker.SelectedDate = DateTimeProvider.ToLocalTime(fullInvoice.InvoiceDate);
        DueDatePicker.SelectedDate = DateTimeProvider.ToLocalTime(fullInvoice.DueDate);
        StatusComboBox.SelectedIndex = (int)fullInvoice.Status;
        NotesTextBox.Text = fullInvoice.Notes;

        foreach (var item in fullInvoice.InvoiceItems)
        {
            _items.Add(item);
        }

        CalculateTotals();
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemDescriptionTextBox.Text))
        {
            MessageBox.Show("Please enter an item description.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(ItemQuantityTextBox.Text, out decimal quantity) || quantity <= 0)
        {
            MessageBox.Show("Please enter a valid quantity.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(ItemPriceTextBox.Text, out decimal price) || price < 0)
        {
            MessageBox.Show("Please enter a valid price.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var lineTotal = quantity * price;
        var gstAmount = _gstService.CalculateGstToAdd(lineTotal, 0.10m);

        var item = new InvoiceItem
        {
            Description = ItemDescriptionTextBox.Text.Trim(),
            ServiceDate = ItemServiceDatePicker.SelectedDate,
            Quantity = quantity,
            UnitPrice = price,
            GSTRate = 0.10m,
            LineTotal = lineTotal,
            GSTAmount = gstAmount
        };

        _items.Add(item);

        ItemDescriptionTextBox.Clear();
        ItemServiceDatePicker.SelectedDate = null;
        ItemQuantityTextBox.Text = "1";
        ItemPriceTextBox.Clear();
        ItemDescriptionTextBox.Focus();

        CalculateTotals();
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is InvoiceItem item)
        {
            _items.Remove(item);
            CalculateTotals();
        }
    }

    private void CalculateTotals(object? sender = null, TextChangedEventArgs? e = null)
    {
        var subtotal = _items.Sum(i => i.LineTotal);
        var gstTotal = _items.Sum(i => i.GSTAmount);
        var total = subtotal + gstTotal;

        SubtotalText.Text = subtotal.ToString("C");
        GSTText.Text = gstTotal.ToString("C");
        TotalText.Text = total.ToString("C");
    }

    /// <summary>
    /// Validates the invoice before saving
    /// </summary>
    private async Task<(bool IsValid, string Error)> ValidateInvoiceAsync()
    {
        // Basic field validation
        if (CustomerComboBox.SelectedItem == null)
            return (false, "Please select a customer.");

        if (string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text))
            return (false, "Please enter an invoice number.");

        if (_items.Count == 0)
            return (false, "Please add at least one item to the invoice.");

        if (!InvoiceDatePicker.SelectedDate.HasValue)
            return (false, "Please select an invoice date.");

        if (!DueDatePicker.SelectedDate.HasValue)
            return (false, "Please select a due date.");

        // Business rule validations
        if (DueDatePicker.SelectedDate < InvoiceDatePicker.SelectedDate)
            return (false, "Due date cannot be before invoice date.");

        if (_items.Any(i => i.Quantity <= 0))
            return (false, "All item quantities must be greater than zero.");

        if (_items.Any(i => i.UnitPrice < 0))
            return (false, "Unit prices cannot be negative.");

        // Check for duplicate invoice number (only when creating new)
        if (!_isEditMode)
        {
            using var context = new AuroraDbContext();
            var exists = await context.Invoices
                .AnyAsync(i => i.InvoiceNumber == InvoiceNumberTextBox.Text.Trim());
            if (exists)
                return (false, $"Invoice number '{InvoiceNumberTextBox.Text.Trim()}' already exists. Please use a different number.");
        }

        return (true, string.Empty);
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validate first
        var (isValid, error) = await ValidateInvoiceAsync();
        if (!isValid)
        {
            MessageBox.Show(error, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

            using var context = _contextFactory.CreateDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var selectedCustomer = (Customer)CustomerComboBox.SelectedItem;

                var subtotal = _items.Sum(i => i.LineTotal);
                var gstTotal = _items.Sum(i => i.GSTAmount);
                var total = subtotal + gstTotal;

                // Convert local DateTime to UTC for storage
                var invoiceDate = DateTimeProvider.ToUtcTime(InvoiceDatePicker.SelectedDate!.Value);
                var dueDate = DateTimeProvider.ToUtcTime(DueDatePicker.SelectedDate!.Value);

                if (_isEditMode && _invoice != null)
                {
                    var invoiceToUpdate = await context.Invoices
                        .Include(i => i.InvoiceItems)
                        .FirstOrDefaultAsync(i => i.Id == _invoice.Id);

                    if (invoiceToUpdate != null)
                    {
                        invoiceToUpdate.CustomerId = selectedCustomer.Id;
                        invoiceToUpdate.InvoiceNumber = InvoiceNumberTextBox.Text.Trim();
                        invoiceToUpdate.InvoiceDate = invoiceDate;
                        invoiceToUpdate.DueDate = dueDate;
                        invoiceToUpdate.Status = (InvoiceStatus)StatusComboBox.SelectedIndex;
                        invoiceToUpdate.SubTotal = subtotal;
                        invoiceToUpdate.GSTAmount = gstTotal;
                        invoiceToUpdate.TotalAmount = total;
                        invoiceToUpdate.Notes = NotesTextBox.Text.Trim();
                        invoiceToUpdate.ModifiedDate = DateTimeProvider.UtcNow;

                        // Remove old items and add new ones
                        context.InvoiceItems.RemoveRange(invoiceToUpdate.InvoiceItems);

                        foreach (var item in _items)
                        {
                            invoiceToUpdate.InvoiceItems.Add(new InvoiceItem
                            {
                                Description = item.Description,
                                ServiceDate = item.ServiceDate,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice,
                                GSTRate = item.GSTRate,
                                LineTotal = item.LineTotal,
                                GSTAmount = item.GSTAmount
                            });
                        }
                    }
                }
                else
                {
                    var newInvoice = new Invoice
                    {
                        CustomerId = selectedCustomer.Id,
                        InvoiceNumber = InvoiceNumberTextBox.Text.Trim(),
                        InvoiceDate = invoiceDate,
                        DueDate = dueDate,
                        Status = (InvoiceStatus)StatusComboBox.SelectedIndex,
                        SubTotal = subtotal,
                        GSTAmount = gstTotal,
                        TotalAmount = total,
                        Notes = NotesTextBox.Text.Trim(),
                        CreatedDate = DateTimeProvider.UtcNow
                    };

                    foreach (var item in _items)
                    {
                        newInvoice.InvoiceItems.Add(item);
                    }

                    context.Invoices.Add(newInvoice);

                    // Update next invoice number
                    var settings = await context.AppSettings.FirstOrDefaultAsync();
                    if (settings != null)
                    {
                        settings.NextInvoiceNumber++;
                        settings.ModifiedDate = DateTimeProvider.UtcNow;
                    }
                }

                // Commit all changes atomically
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                DialogResult = true;
                Close();
            }
            catch (Exception)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            await LoggingService.LogErrorAsync(ex, "InvoiceDialog.Save_Click");
            MessageBox.Show($"Error saving invoice: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
