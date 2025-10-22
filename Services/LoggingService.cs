using System.Diagnostics;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Common;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for logging errors and events to the database
/// </summary>
public class LoggingService
{
    /// <summary>
    /// Log an error to the database
    /// </summary>
    public static async Task LogErrorAsync(Exception ex, string source, string? userAction = null)
    {
        try
        {
            using var context = new AuroraDbContext();

            var errorLog = new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Error",
                Source = source,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                AdditionalInfo = ex.InnerException?.Message,
                UserAction = userAction,
                IsResolved = false
            };

            context.ErrorLogs.Add(errorLog);
            await context.SaveChangesAsync();

            // Also write to debug output
            Debug.WriteLine($"[ERROR] {source}: {ex.Message}");
        }
        catch (Exception loggingEx)
        {
            // If logging fails, write to debug output
            Debug.WriteLine($"[LOGGING FAILED] {loggingEx.Message}");
            Debug.WriteLine($"[ORIGINAL ERROR] {source}: {ex.Message}");
        }
    }

    /// <summary>
    /// Log a critical error to the database
    /// </summary>
    public static async Task LogCriticalAsync(Exception ex, string source, string? userAction = null)
    {
        try
        {
            using var context = new AuroraDbContext();

            var errorLog = new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Critical",
                Source = source,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                AdditionalInfo = ex.InnerException?.Message,
                UserAction = userAction,
                IsResolved = false
            };

            context.ErrorLogs.Add(errorLog);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[CRITICAL] {source}: {ex.Message}");
        }
        catch (Exception loggingEx)
        {
            Debug.WriteLine($"[LOGGING FAILED] {loggingEx.Message}");
            Debug.WriteLine($"[ORIGINAL CRITICAL] {source}: {ex.Message}");
        }
    }

    /// <summary>
    /// Log a warning to the database
    /// </summary>
    public static async Task LogWarningAsync(string message, string source, string? additionalInfo = null)
    {
        try
        {
            using var context = new AuroraDbContext();

            var errorLog = new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Warning",
                Source = source,
                Message = message,
                AdditionalInfo = additionalInfo,
                IsResolved = false
            };

            context.ErrorLogs.Add(errorLog);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[WARNING] {source}: {message}");
        }
        catch (Exception loggingEx)
        {
            Debug.WriteLine($"[LOGGING FAILED] {loggingEx.Message}");
        }
    }

    /// <summary>
    /// Log informational message to the database
    /// </summary>
    public static async Task LogInfoAsync(string message, string source, string? additionalInfo = null)
    {
        try
        {
            using var context = new AuroraDbContext();

            var errorLog = new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Info",
                Source = source,
                Message = message,
                AdditionalInfo = additionalInfo,
                IsResolved = true // Info logs are auto-resolved
            };

            context.ErrorLogs.Add(errorLog);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[INFO] {source}: {message}");
        }
        catch (Exception loggingEx)
        {
            Debug.WriteLine($"[LOGGING FAILED] {loggingEx.Message}");
        }
    }

    /// <summary>
    /// Get recent error logs
    /// </summary>
    public static async Task<List<ErrorLog>> GetRecentErrorsAsync(int count = 100)
    {
        try
        {
            using var context = new AuroraDbContext();
            return await Task.Run(() =>
                context.ErrorLogs
                    .OrderByDescending(e => e.Timestamp)
                    .Take(count)
                    .ToList()
            );
        }
        catch
        {
            return new List<ErrorLog>();
        }
    }

    /// <summary>
    /// Clear old resolved logs (keeps last 30 days)
    /// </summary>
    public static async Task CleanupOldLogsAsync()
    {
        try
        {
            using var context = new AuroraDbContext();
            var cutoffDate = DateTimeProvider.UtcNow.AddDays(-AppConstants.DefaultLogRetentionDays);

            var oldLogs = context.ErrorLogs
                .Where(e => e.IsResolved && e.Timestamp < cutoffDate)
                .ToList();

            if (oldLogs.Any())
            {
                context.ErrorLogs.RemoveRange(oldLogs);
                await context.SaveChangesAsync();
                Debug.WriteLine($"[CLEANUP] Removed {oldLogs.Count} old log entries");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CLEANUP FAILED] {ex.Message}");
        }
    }
}
