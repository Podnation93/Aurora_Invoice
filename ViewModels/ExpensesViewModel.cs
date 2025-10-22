using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AuroraInvoice.Data;

namespace AuroraInvoice.ViewModels;

public partial class ExpensesViewModel : ObservableObject
{
    private readonly IDbContextFactory<AuroraDbContext> _contextFactory;

    [ObservableProperty]
    private ObservableCollection<Expense> _expenses = new();

    [ObservableProperty]
    private ObservableCollection<ExpenseCategory> _categories = new();

    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private decimal _totalGst;

    [ObservableProperty]
    private int _expenseCount;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ExpenseCategory? _selectedCategory = null;

    public ICommand NewExpenseCommand { get; }
    public ICommand<Expense> EditExpenseCommand { get; }
    public ICommand<Expense> DeleteExpenseCommand { get; }

    public ExpensesViewModel(IDbContextFactory<AuroraDbContext> contextFactory)
    {
        _contextFactory = contextFactory;

        NewExpenseCommand = new AsyncRelayCommand(CreateNewExpense);
        EditExpenseCommand = new AsyncRelayCommand<Expense>(EditExpense);
        DeleteExpenseCommand = new AsyncRelayCommand<Expense>(DeleteExpense);

        LoadData();
    }

    private async Task LoadData()
    {
        using var context = _contextFactory.CreateDbContext();
        var allExpenses = await context.Expenses.Include(e => e.Category).OrderByDescending(e => e.Date).ToListAsync();
        
        var filteredExpenses = allExpenses.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredExpenses = filteredExpenses.Where(e =>
                e.Vendor.ToLower().Contains(SearchText.ToLower()) ||
                e.Description.ToLower().Contains(SearchText.ToLower()) ||
                e.Category.Name.ToLower().Contains(SearchText.ToLower())
            );
        }

        if (SelectedCategory != null)
        {
            filteredExpenses = filteredExpenses.Where(e => e.CategoryId == SelectedCategory.Id);
        }

        Expenses = new ObservableCollection<Expense>(filteredExpenses);

        var now = System.DateTime.Now;
        var firstDay = new System.DateTime(now.Year, now.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        var monthlyExpenses = allExpenses.Where(e => e.Date >= firstDay && e.Date <= lastDay);

        TotalExpenses = monthlyExpenses.Sum(e => e.Amount);
        TotalGst = monthlyExpenses.Sum(e => e.GSTAmount);
        ExpenseCount = monthlyExpenses.Count();

        if (Categories.Count == 0)
        {
            var categories = await context.ExpenseCategories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            Categories = new ObservableCollection<ExpenseCategory>(categories);
        }
    }

    private async Task CreateNewExpense()
    {
        // Requires dialog service
        await LoadData();
    }

    private async Task EditExpense(Expense? expense)
    {
        if (expense == null) return;
        // Requires dialog service
        await LoadData();
    }

    private async Task DeleteExpense(Expense? expense)
    {        
        if (expense == null) return;
        using var context = _contextFactory.CreateDbContext();
        context.Expenses.Remove(expense);
        await context.SaveChangesAsync();
        await LoadData();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadData();
    }

    partial void OnSelectedCategoryChanged(ExpenseCategory? value)
    {
        _ = LoadData();
    }
}
