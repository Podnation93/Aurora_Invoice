using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for audit logging of business operations
/// </summary>
public class AuditService : IAuditService
{
    /// <summary>
    /// Logs an audit entry for a business operation
    /// </summary>
    /// <param name="action">Action performed (Create, Update, Delete, etc.)</param>
    /// <param name="entityType">Type of entity affected (Invoice, Customer, etc.)</param>
    /// <param name="entityId">ID of the affected entity</param>
    /// <param name="details">Optional JSON details of the operation</param>
    /// <param name="userName">Optional user name (for future multi-user support)</param>
    public async Task LogAuditAsync(string action, string entityType, int entityId, string? details = null, string? userName = null)
    {
        try
        {
            using var context = new AuroraDbContext();

            var auditLog = new AuditLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                UserName = userName ?? Environment.UserName,
                Success = true
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[AUDIT] {action} {entityType} #{entityId} by {auditLog.UserName}");
        }
        catch (Exception ex)
        {
            // If audit logging fails, log to debug but don't throw
            Debug.WriteLine($"[AUDIT FAILED] {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a failed operation audit entry
    /// </summary>
    /// <param name="action">Action attempted</param>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="userName">Optional user name</param>
    public async Task LogFailedAuditAsync(string action, string entityType, int entityId, string errorMessage, string? userName = null)
    {
        try
        {
            using var context = new AuroraDbContext();

            var auditLog = new AuditLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserName = userName ?? Environment.UserName,
                Success = false,
                ErrorMessage = errorMessage
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[AUDIT FAILED] {action} {entityType} #{entityId}: {errorMessage}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AUDIT LOGGING FAILED] {ex.Message}");
        }
    }

    /// <summary>
    /// Gets recent audit entries
    /// </summary>
    /// <param name="count">Number of entries to retrieve</param>
    /// <param name="entityType">Optional filter by entity type</param>
    /// <returns>List of audit log entries</returns>
    public async Task<List<AuditLog>> GetRecentAuditsAsync(int count = 100, string? entityType = null)
    {
        try
        {
            using var context = new AuroraDbContext();
            var query = context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        catch
        {
            return new List<AuditLog>();
        }
    }

    /// <summary>
    /// Cleans up old audit logs based on retention policy
    /// </summary>
    /// <param name="retentionDays">Number of days to retain logs</param>
    public async Task CleanupOldAuditsAsync(int retentionDays)
    {
        try
        {
            using var context = new AuroraDbContext();
            var cutoffDate = DateTimeProvider.UtcNow.AddDays(-retentionDays);

            var oldAudits = await context.AuditLogs
                .Where(a => a.Timestamp < cutoffDate)
                .ToListAsync();

            if (oldAudits.Any())
            {
                context.AuditLogs.RemoveRange(oldAudits);
                await context.SaveChangesAsync();
                Debug.WriteLine($"[AUDIT CLEANUP] Removed {oldAudits.Count} old audit entries");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AUDIT CLEANUP FAILED] {ex.Message}");
        }
    }
}
