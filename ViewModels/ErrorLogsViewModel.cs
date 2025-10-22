using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace AuroraInvoice.ViewModels;

public partial class ErrorLogsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ErrorLog> _logs = new();

    [ObservableProperty]
    private string? _selectedSeverity;

    public ICommand RefreshCommand { get; }
    public ICommand ClearOldLogsCommand { get; }

    public ErrorLogsViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(LoadLogs);
        ClearOldLogsCommand = new AsyncRelayCommand(ClearOldLogs);
        LoadLogs();
    }

    private async Task LoadLogs()
    {
        var allLogs = await LoggingService.GetRecentErrorsAsync(500);
        var filteredLogs = allLogs.AsEnumerable();

        if (!string.IsNullOrEmpty(SelectedSeverity) && SelectedSeverity != "All Severity")
        {
            filteredLogs = filteredLogs.Where(l => l.Severity == SelectedSeverity);
        }

        Logs = new ObservableCollection<ErrorLog>(filteredLogs);
    }

    private async Task ClearOldLogs()
    {
        await LoggingService.CleanupOldLogsAsync();
        await LoadLogs();
    }

    partial void OnSelectedSeverityChanged(string? value)
    {
        _ = LoadLogs();
    }
}
