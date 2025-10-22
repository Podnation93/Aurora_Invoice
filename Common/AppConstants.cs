namespace AuroraInvoice.Common;

/// <summary>
/// Application-wide constants to avoid magic numbers and strings
/// </summary>
public static class AppConstants
{
    // Database
    public const string DatabaseFileName = "aurora_invoice.db";
    public const string AppFolderName = "AuroraInvoice";
    public const string BackupFolderName = "Backups";

    // Invoice Settings
    public const int DefaultPaymentTermsDays = 30;
    public const decimal DefaultGstRate = 0.10m;
    public const string DefaultInvoicePrefix = "INV-";
    public const int DefaultInvoiceNumberStart = 1;

    // UI Settings
    public const int DashboardRecentInvoicesCount = 10;
    public const int DefaultPageSize = 50;
    public const int SearchTextMaxLength = 200;

    // Logging
    public const int DefaultLogRetentionDays = 30;
    public const int MaxRecentLogsToFetch = 100;
    public const int ErrorQueueProcessingDelayMs = 100;

    // PDF Generation
    public const string DefaultPdfFontFamily = "Arial";
    public const int DefaultPdfFontSize = 11;

    // Validation
    public const int MaxDescriptionLength = 500;
    public const int MaxNameLength = 200;
    public const int MaxEmailLength = 200;

    // Backup
    public const string BackupFilePattern = "aurora_invoice_backup_*.zip";
    public const string BackupTimestampFormat = "yyyyMMdd_HHmmss";
    public const string PreRestoreBackupSuffix = ".pre_restore_backup";
}
