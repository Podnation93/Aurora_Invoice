using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;

namespace AuroraInvoice.Views;

public partial class ExpensesPage : Page
{
    private List<Expense> _allExpenses = new();
    private List<ExpenseCategory> _categories = new();

    public ExpensesPage()
    {
        InitializeComponent();
        Loaded += ExpensesPage_Loaded;
    }

    private async void ExpensesPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCategoriesAsync();
        await LoadExpensesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        using var context = new AuroraDbContext();
        _categories = await context.ExpenseCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        foreach (var category in _categories)
        {
            CategoryFilterComboBox.Items.Add(new ComboBoxItem
            {
                Content = category.Name,
                Tag = category.Id
            });
        }
    }

    private async Task LoadExpensesAsync()
    {
        try
        {
            using var context = new AuroraDbContext();

            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            _allExpenses = await context.Expenses
                .Include(e => e.Category)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            ExpensesGrid.ItemsSource = _allExpenses;

            var monthlyExpenses = _allExpenses
                .Where(e => e.Date >= firstDay && e.Date <= lastDay);

            TotalExpensesText.Text = monthlyExpenses.Sum(e => e.Amount).ToString("C");
            TotalGSTText.Text = monthlyExpenses.Sum(e => e.GSTAmount).ToString("C");
            ExpenseCountText.Text = monthlyExpenses.Count().ToString();

            EmptyState.Visibility = _allExpenses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ExpensesGrid.Visibility = _allExpenses.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading expenses: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterExpenses();
    }

    private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterExpenses();
    }

    private void FilterExpenses()
    {
        var searchText = SearchBox.Text.ToLower();
        var filtered = _allExpenses.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered.Where(e =>
                e.Vendor.ToLower().Contains(searchText) ||
                e.Description.ToLower().Contains(searchText) ||
                e.Category.Name.ToLower().Contains(searchText)
            );
        }

        if (CategoryFilterComboBox.SelectedIndex > 0)
        {
            var selectedItem = CategoryFilterComboBox.SelectedItem as ComboBoxItem;
            var categoryId = (int)selectedItem!.Tag;
            filtered = filtered.Where(e => e.CategoryId == categoryId);
        }

        ExpensesGrid.ItemsSource = filtered.ToList();
    }

    private void NewExpense_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ExpenseDialog(_categories);
        if (dialog.ShowDialog() == true)
        {
            _ = LoadExpensesAsync();
        }
    }

    private void EditExpense_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Expense expense)
        {
            var dialog = new ExpenseDialog(expense, _categories);
            if (dialog.ShowDialog() == true)
            {
                _ = LoadExpensesAsync();
            }
        }
    }

    private async void DeleteExpense_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Expense expense)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete this expense?\n\nVendor: {expense.Vendor}\nAmount: {expense.Amount:C}",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new AuroraDbContext();
                    context.Expenses.Remove(expense);
                    await context.SaveChangesAsync();

                    MessageBox.Show("Expense deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadExpensesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting expense: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
