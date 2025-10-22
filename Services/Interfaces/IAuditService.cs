namespace AuroraInvoice.Services.Interfaces;

/// <summary>
/// Service interface for audit logging of business operations
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry for a business operation
    /// </summary>
    /// <param name="action">Action performed (Create, Update, Delete, etc.)</param>
    /// <param name="entityType">Type of entity affected (Invoice, Customer, etc.)</param>
    /// <param name="entityId">ID of the affected entity</param>
    /// <param name="details">Optional JSON details of the operation</param>
    /// <param name="userName">Optional user name (for future multi-user support)</param>
    Task LogAuditAsync(string action, string entityType, int entityId, string? details = null, string? userName = null);

    /// <summary>
    /// Gets recent audit entries
    /// </summary>
    /// <param name="count">Number of entries to retrieve</param>
    /// <param name="entityType">Optional filter by entity type</param>
    /// <returns>List of audit log entries</returns>
    Task<List<Models.AuditLog>> GetRecentAuditsAsync(int count = 100, string? entityType = null);

    /// <summary>
    /// Cleans up old audit logs based on retention policy
    /// </summary>
    /// <param name="retentionDays">Number of days to retain logs</param>
    Task CleanupOldAuditsAsync(int retentionDays);
}
