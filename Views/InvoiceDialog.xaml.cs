using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services;

namespace AuroraInvoice.Views;

public partial class InvoiceDialog : Window
{
    private Invoice? _invoice;
    private bool _isEditMode;
    private List<Customer> _customers;
    private ObservableCollection<InvoiceItem> _items = new();
    private readonly GstCalculationService _gstService;

    public InvoiceDialog(List<Customer> customers)
    {
        InitializeComponent();
        _customers = customers;
        _gstService = new GstCalculationService();
        _isEditMode = false;
        HeaderText.Text = "New Invoice";

        CustomerComboBox.ItemsSource = _customers;
        if (_customers.Count > 0)
            CustomerComboBox.SelectedIndex = 0;

        ItemsGrid.ItemsSource = _items;

        InvoiceDatePicker.SelectedDate = DateTime.Now;
        DueDatePicker.SelectedDate = DateTime.Now.AddDays(30);
        GenerateInvoiceNumber();
    }

    public InvoiceDialog(Invoice invoice, List<Customer> customers)
    {
        InitializeComponent();
        _invoice = invoice;
        _customers = customers;
        _gstService = new GstCalculationService();
        _isEditMode = true;
        HeaderText.Text = "Edit Invoice";

        CustomerComboBox.ItemsSource = _customers;
        ItemsGrid.ItemsSource = _items;

        LoadInvoiceData();
    }

    private async void GenerateInvoiceNumber()
    {
        using var context = new AuroraDbContext();
        var settings = await context.AppSettings.FirstOrDefaultAsync();

        if (settings != null)
        {
            InvoiceNumberTextBox.Text = $"{settings.InvoicePrefix}{settings.NextInvoiceNumber:D4}";
        }
        else
        {
            InvoiceNumberTextBox.Text = "INV-0001";
        }
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
        InvoiceDatePicker.SelectedDate = fullInvoice.InvoiceDate;
        DueDatePicker.SelectedDate = fullInvoice.DueDate;
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
            Quantity = quantity,
            UnitPrice = price,
            GSTRate = 0.10m,
            LineTotal = lineTotal,
            GSTAmount = gstAmount
        };

        _items.Add(item);

        ItemDescriptionTextBox.Clear();
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

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (CustomerComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a customer.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text))
        {
            MessageBox.Show("Please enter an invoice number.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_items.Count == 0)
        {
            MessageBox.Show("Please add at least one item to the invoice.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!InvoiceDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Please select an invoice date.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!DueDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Please select a due date.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new AuroraDbContext();
            var selectedCustomer = (Customer)CustomerComboBox.SelectedItem;

            var subtotal = _items.Sum(i => i.LineTotal);
            var gstTotal = _items.Sum(i => i.GSTAmount);
            var total = subtotal + gstTotal;

            if (_isEditMode && _invoice != null)
            {
                var invoiceToUpdate = await context.Invoices
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == _invoice.Id);

                if (invoiceToUpdate != null)
                {
                    invoiceToUpdate.CustomerId = selectedCustomer.Id;
                    invoiceToUpdate.InvoiceNumber = InvoiceNumberTextBox.Text.Trim();
                    invoiceToUpdate.InvoiceDate = InvoiceDatePicker.SelectedDate.Value;
                    invoiceToUpdate.DueDate = DueDatePicker.SelectedDate.Value;
                    invoiceToUpdate.Status = (InvoiceStatus)StatusComboBox.SelectedIndex;
                    invoiceToUpdate.SubTotal = subtotal;
                    invoiceToUpdate.GSTAmount = gstTotal;
                    invoiceToUpdate.TotalAmount = total;
                    invoiceToUpdate.Notes = NotesTextBox.Text.Trim();
                    invoiceToUpdate.ModifiedDate = DateTime.Now;

                    context.InvoiceItems.RemoveRange(invoiceToUpdate.InvoiceItems);

                    foreach (var item in _items)
                    {
                        invoiceToUpdate.InvoiceItems.Add(new InvoiceItem
                        {
                            Description = item.Description,
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
                    InvoiceDate = InvoiceDatePicker.SelectedDate.Value,
                    DueDate = DueDatePicker.SelectedDate.Value,
                    Status = (InvoiceStatus)StatusComboBox.SelectedIndex,
                    SubTotal = subtotal,
                    GSTAmount = gstTotal,
                    TotalAmount = total,
                    Notes = NotesTextBox.Text.Trim(),
                    CreatedDate = DateTime.Now
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
                }
            }

            await context.SaveChangesAsync();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
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
