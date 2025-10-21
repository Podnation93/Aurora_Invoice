using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services;

namespace AuroraInvoice.Views;

public partial class ErrorLogsPage : Page
{
    private List<ErrorLog> _allLogs = new();

    public ErrorLogsPage()
    {
        InitializeComponent();
        Loaded += ErrorLogsPage_Loaded;
    }

    private async void ErrorLogsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            _allLogs = await LoggingService.GetRecentErrorsAsync(500);
            FilterLogs();

            EmptyState.Visibility = _allLogs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            LogsGrid.Visibility = _allLogs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading logs: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SeverityFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterLogs();
    }

    private void FilterLogs()
    {
        if (SeverityFilterComboBox == null)
            return;

        var filtered = _allLogs.AsEnumerable();

        if (SeverityFilterComboBox.SelectedIndex > 0)
        {
            var selectedSeverity = SeverityFilterComboBox.SelectedIndex switch
            {
                1 => "Critical",
                2 => "Error",
                3 => "Warning",
                4 => "Info",
                _ => ""
            };
            filtered = filtered.Where(l => l.Severity == selectedSeverity);
        }

        LogsGrid.ItemsSource = filtered.ToList();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadLogsAsync();
    }

    private async void ClearOldLogs_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will remove all resolved logs older than 30 days.\n\nDo you want to continue?",
            "Confirm Cleanup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await LoggingService.CleanupOldLogsAsync();
            await LoadLogsAsync();
            MessageBox.Show("Old logs have been cleaned up.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void LogsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LogsGrid.SelectedItem is ErrorLog log)
        {
            var details = $"Timestamp: {log.Timestamp:dd/MM/yyyy HH:mm:ss}\n" +
                         $"Severity: {log.Severity}\n" +
                         $"Source: {log.Source}\n" +
                         $"Message: {log.Message}\n\n";

            if (!string.IsNullOrEmpty(log.StackTrace))
                details += $"Stack Trace:\n{log.StackTrace}\n\n";

            if (!string.IsNullOrEmpty(log.AdditionalInfo))
                details += $"Additional Info:\n{log.AdditionalInfo}\n\n";

            if (!string.IsNullOrEmpty(log.UserAction))
                details += $"User Action: {log.UserAction}";

            MessageBox.Show(details, "Error Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
