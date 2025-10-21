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

        // Configure QuestPDF license (Community license for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;

        // Initialize database
        await InitializeDatabaseAsync();
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

