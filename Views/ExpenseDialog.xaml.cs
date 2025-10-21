using System.Windows;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services;

namespace AuroraInvoice.Views;

public partial class ExpenseDialog : Window
{
    private Expense? _expense;
    private bool _isEditMode;
    private List<ExpenseCategory> _categories;
    private readonly GstCalculationService _gstService;

    public ExpenseDialog(List<ExpenseCategory> categories)
    {
        InitializeComponent();
        _categories = categories;
        _gstService = new GstCalculationService();
        _isEditMode = false;
        HeaderText.Text = "New Expense";
        CategoryComboBox.ItemsSource = _categories;
        if (_categories.Count > 0)
            CategoryComboBox.SelectedIndex = 0;
    }

    public ExpenseDialog(Expense expense, List<ExpenseCategory> categories)
    {
        InitializeComponent();
        _expense = expense;
        _categories = categories;
        _gstService = new GstCalculationService();
        _isEditMode = true;
        HeaderText.Text = "Edit Expense";

        CategoryComboBox.ItemsSource = _categories;

        DatePicker.SelectedDate = expense.Date;
        VendorTextBox.Text = expense.Vendor;
        DescriptionTextBox.Text = expense.Description;
        AmountTextBox.Text = expense.Amount.ToString("F2");
        NotesTextBox.Text = expense.Notes;

        var category = _categories.FirstOrDefault(c => c.Id == expense.CategoryId);
        if (category != null)
            CategoryComboBox.SelectedItem = category;
    }

    private void AmountTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
        {
            var gstAmount = _gstService.CalculateGstFromTotal(amount, 0.10m);
            GSTInfoText.Text = $"GST: {gstAmount:C} (10%)";
        }
        else
        {
            GSTInfoText.Text = "GST: $0.00 (10%)";
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!DatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Please select a date.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(VendorTextBox.Text))
        {
            MessageBox.Show("Please enter a vendor name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            VendorTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
        {
            MessageBox.Show("Please enter a description.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            DescriptionTextBox.Focus();
            return;
        }

        if (CategoryComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a category.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
        {
            MessageBox.Show("Please enter a valid amount.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            AmountTextBox.Focus();
            return;
        }

        try
        {
            using var context = new AuroraDbContext();
            var selectedCategory = (ExpenseCategory)CategoryComboBox.SelectedItem;
            var gstAmount = _gstService.CalculateGstFromTotal(amount, 0.10m);

            if (_isEditMode && _expense != null)
            {
                var expenseToUpdate = await context.Expenses.FindAsync(_expense.Id);
                if (expenseToUpdate != null)
                {
                    expenseToUpdate.Date = DatePicker.SelectedDate.Value;
                    expenseToUpdate.Vendor = VendorTextBox.Text.Trim();
                    expenseToUpdate.Description = DescriptionTextBox.Text.Trim();
                    expenseToUpdate.Amount = amount;
                    expenseToUpdate.GSTAmount = gstAmount;
                    expenseToUpdate.CategoryId = selectedCategory.Id;
                    expenseToUpdate.Notes = NotesTextBox.Text.Trim();
                    expenseToUpdate.ModifiedDate = DateTime.Now;
                }
            }
            else
            {
                var newExpense = new Expense
                {
                    Date = DatePicker.SelectedDate.Value,
                    Vendor = VendorTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    Amount = amount,
                    GSTAmount = gstAmount,
                    CategoryId = selectedCategory.Id,
                    Notes = NotesTextBox.Text.Trim(),
                    CreatedDate = DateTime.Now
                };

                context.Expenses.Add(newExpense);
            }

            await context.SaveChangesAsync();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving expense: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
