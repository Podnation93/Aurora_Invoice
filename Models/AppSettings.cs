using System.ComponentModel.DataAnnotations;
using AuroraInvoice.Common;

namespace AuroraInvoice.Models;

/// <summary>
/// Application settings and preferences
/// </summary>
public class AppSettings
{
    [Key]
    public int Id { get; set; }

    // Business Information
    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ABN { get; set; }

    [MaxLength(500)]
    public string? BusinessAddress { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(1000)]
    public string? LogoPath { get; set; }

    // Invoice Settings
    [MaxLength(20)]
    public string? InvoicePrefix { get; set; } = "INV-";

    public int NextInvoiceNumber { get; set; } = 1;

    public int DefaultPaymentTermsDays { get; set; } = 30;

    // Default GST Rate (as decimal, e.g., 0.10 for 10%)
    public decimal DefaultGSTRate { get; set; } = 0.10m;

    // Backup Settings
    [MaxLength(1000)]
    public string? BackupFolderPath { get; set; }

    public bool AutoBackupEnabled { get; set; } = false;

    public int AutoBackupIntervalDays { get; set; } = 7;

    public DateTime? LastBackupDate { get; set; }

    // Theme Settings
    [MaxLength(50)]
    public string? ThemeColor { get; set; } = "#2563eb"; // Blue

    [MaxLength(50)]
    public string? AccentColor { get; set; } = "#7c3aed"; // Purple

    public DateTime CreatedDate { get; set; } = DateTimeProvider.UtcNow;

    public DateTime? ModifiedDate { get; set; }
}
