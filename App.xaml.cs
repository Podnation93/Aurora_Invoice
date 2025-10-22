using System.Collections.Concurrent;
using System.Windows;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services;
using AuroraInvoice.Common;
using QuestPDF.Infrastructure;

namespace AuroraInvoice;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly ConcurrentQueue<ErrorLog> _errorQueue = new();
    private static CancellationTokenSource? _errorProcessingCts;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up global exception handlers
        SetupExceptionHandlers();

        // Start error queue processor
        _errorProcessingCts = new CancellationTokenSource();
        _ = Task.Run(() => ProcessErrorQueueAsync(_errorProcessingCts.Token));

        // Configure QuestPDF license (Community license for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;

        // Initialize database
        await InitializeDatabaseAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop error processing and wait for queue to flush
        _errorProcessingCts?.Cancel();

        // Give it a moment to flush remaining errors
        Thread.Sleep(500);

        base.OnExit(e);
    }

    private void SetupExceptionHandlers()
    {
        // Handle unhandled exceptions in the UI thread
        DispatcherUnhandledException += (sender, e) =>
        {
            _errorQueue.Enqueue(new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Critical",
                Source = "App.DispatcherUnhandledException",
                Message = e.Exception.Message,
                StackTrace = e.Exception.StackTrace,
                AdditionalInfo = e.Exception.InnerException?.Message,
                UserAction = "Unhandled exception in UI thread",
                IsResolved = false
            });

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true; // Prevent application crash
        };

        // Handle unhandled exceptions in background threads
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                _errorQueue.Enqueue(new ErrorLog
                {
                    Timestamp = DateTimeProvider.UtcNow,
                    Severity = "Critical",
                    Source = "AppDomain.UnhandledException",
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    AdditionalInfo = ex.InnerException?.Message,
                    UserAction = "Unhandled exception in background thread",
                    IsResolved = false
                });
            }
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            _errorQueue.Enqueue(new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Error",
                Source = "TaskScheduler.UnobservedTaskException",
                Message = e.Exception.Message,
                StackTrace = e.Exception.StackTrace,
                AdditionalInfo = e.Exception.InnerException?.Message,
                UserAction = "Unobserved task exception",
                IsResolved = false
            });

            e.SetObserved(); // Prevent application crash
        };
    }

    /// <summary>
    /// Background task that processes the error queue and writes to database
    /// This prevents race conditions and database locks from concurrent exception handlers
    /// </summary>
    private async Task ProcessErrorQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_errorQueue.TryDequeue(out var errorLog))
            {
                try
                {
                    using var context = new AuroraDbContext();
                    context.ErrorLogs.Add(errorLog);
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception)
                {
                    // If logging fails, re-queue and wait longer
                    _errorQueue.Enqueue(errorLog);
                    await Task.Delay(1000, cancellationToken);
                }
            }
            else
            {
                // No errors in queue, wait before checking again
                await Task.Delay(AppConstants.ErrorQueueProcessingDelayMs, cancellationToken);
            }
        }

        // On cancellation, try to flush remaining errors
        while (_errorQueue.TryDequeue(out var errorLog))
        {
            try
            {
                using var context = new AuroraDbContext();
                context.ErrorLogs.Add(errorLog);
                await context.SaveChangesAsync(CancellationToken.None);
            }
            catch
            {
                // Can't do much at shutdown
                break;
            }
        }
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

