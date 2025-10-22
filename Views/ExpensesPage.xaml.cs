using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class ExpensesPage : Page
{
    private readonly IDbContextFactory<AuroraDbContext> _contextFactory;
    private readonly ISettingsService _settingsService;
    private readonly ILoggingService _loggingService;
    private List<Expense> _allExpenses = new();
    private List<ExpenseCategory> _categories = new();

    public ExpensesPage(IDbContextFactory<AuroraDbContext> contextFactory, ISettingsService settingsService, ILoggingService loggingService)
    {
        InitializeComponent();
        _contextFactory = contextFactory;
        _settingsService = settingsService;
        _loggingService = loggingService;
        Loaded += ExpensesPage_Loaded;
    }

}

