using System;
using System.Threading.Tasks;

namespace AuroraInvoice.Services.Interfaces
{
    public interface ILoggingService
    {
        Task LogErrorAsync(Exception ex, string source, string? userAction = null);
        Task LogCriticalAsync(Exception ex, string source, string? userAction = null);
        Task LogWarningAsync(string message, string source, string? additionalInfo = null);
        Task LogInfoAsync(string message, string source, string? additionalInfo = null);
        Task<System.Collections.Generic.List<AuroraInvoice.Models.ErrorLog>> GetRecentErrorsAsync(int count = 100);
        Task CleanupOldLogsAsync();
    }
}
