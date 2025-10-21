using System.Windows;
using AuroraInvoice.Data;
using AuroraInvoice.Services;
using QuestPDF.Infrastructure;

namespace AuroraInvoice;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up global exception handlers
        SetupExceptionHandlers();

        // Configure QuestPDF license (Community license for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;

        // Initialize database
        await InitializeDatabaseAsync();
    }

    private void SetupExceptionHandlers()
    {
        // Handle unhandled exceptions in the UI thread
        DispatcherUnhandledException += async (sender, e) =>
        {
            await LoggingService.LogCriticalAsync(
                e.Exception,
                "App.DispatcherUnhandledException",
                "Unhandled exception in UI thread");

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true; // Prevent application crash
        };

        // Handle unhandled exceptions in background threads
        AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                await LoggingService.LogCriticalAsync(
                    ex,
                    "AppDomain.UnhandledException",
                    "Unhandled exception in background thread");
            }
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += async (sender, e) =>
        {
            await LoggingService.LogErrorAsync(
                e.Exception,
                "TaskScheduler.UnobservedTaskException",
                "Unobserved task exception");

            e.SetObserved(); // Prevent application crash
        };
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var context = new AuroraDbContext();
            var dbService = new DatabaseService(context);
            await dbService.InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize database: {ex.Message}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Current.Shutdown();
        }
    }
}

