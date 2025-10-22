using System.ComponentModel.DataAnnotations;

namespace AuroraInvoice.Models;

/// <summary>
/// Represents an audit log entry for business operations
/// </summary>
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Timestamp of the audit entry (stored in UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Action performed (Create, Update, Delete, Export, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity affected (Invoice, Customer, Expense, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the affected entity
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Optional JSON details of what changed or operation details
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// User who performed the action (for future multi-user support)
    /// </summary>
    [MaxLength(200)]
    public string? UserName { get; set; }

    /// <summary>
    /// Success status of the operation
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
