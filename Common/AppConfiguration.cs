namespace AuroraInvoice.Common;

/// <summary>
/// Application configuration settings
/// </summary>
public class AppConfiguration
{
    public DatabaseConfiguration Database { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    public UIConfiguration UI { get; set; } = new();
}

/// <summary>
/// Database configuration settings
/// </summary>
public class DatabaseConfiguration
{
    public string Provider { get; set; } = "SQLite";
    public string? FilePath { get; set; }
    public string? BackupPath { get; set; }
    public string ConnectionString { get; set; } = "Cache=Shared;Mode=ReadWriteCreate";
}

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingConfiguration
{
    public int RetentionDays { get; set; } = AppConstants.DefaultLogRetentionDays;
    public int MaxEntries { get; set; } = 10000;
    public bool EnableDebugLogging { get; set; } = false;
}

/// <summary>
/// UI configuration settings
/// </summary>
public class UIConfiguration
{
    public int DashboardRecentInvoices { get; set; } = AppConstants.DashboardRecentInvoicesCount;
    public int PageSize { get; set; } = AppConstants.DefaultPageSize;
    public bool EnableAnimations { get; set; } = true;
}
