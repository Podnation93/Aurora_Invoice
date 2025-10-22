using System.Diagnostics;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Common;
using AuroraInvoice.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuroraInvoice.Services;

public class LoggingService : ILoggingService
{
    private readonly IDbContextFactory<AuroraDbContext> _contextFactory;

    public LoggingService(IDbContextFactory<AuroraDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task LogErrorAsync(Exception ex, string source, string? userAction = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

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

            Debug.WriteLine($"[ERROR] {source}: {ex.Message}");
        }
        catch (Exception loggingEx)
        {
            Debug.WriteLine($"[LOGGING FAILED] {loggingEx.Message}");
            Debug.WriteLine($"[ORIGINAL ERROR] {source}: {ex.Message}");
        }
    }

    public async Task LogCriticalAsync(Exception ex, string source, string? userAction = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

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

    public async Task LogWarningAsync(string message, string source, string? additionalInfo = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

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

    public async Task LogInfoAsync(string message, string source, string? additionalInfo = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

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

    public async Task<System.Collections.Generic.List<ErrorLog>> GetRecentErrorsAsync(int count = 100)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.ErrorLogs
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task CleanupOldLogsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        var cutoffDate = DateTimeProvider.UtcNow.AddDays(-AppConstants.DefaultLogRetentionDays);

        var oldLogs = await context.ErrorLogs
            .Where(e => e.IsResolved && e.Timestamp < cutoffDate)
            .ToListAsync();

        if (oldLogs.Any())
        {
            context.ErrorLogs.RemoveRange(oldLogs);
            await context.SaveChangesAsync();
        }
    }
}
