using Microsoft.Extensions.Configuration;

namespace AuroraInvoice.Common;

/// <summary>
/// Application configuration settings loaded from appsettings.json
/// </summary>
public class AppConfiguration
{
    private static readonly Lazy<AppConfiguration> _lazyInstance = new(() => LoadConfiguration());

    public DatabaseConfiguration Database { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    public UIConfiguration UI { get; set; } = new();
    public BusinessConfiguration Business { get; set; } = new();
    public PerformanceConfiguration Performance { get; set; } = new();
    public BackupConfiguration Backup { get; set; } = new();
    public TemplatesConfiguration Templates { get; set; } = new();

    /// <summary>
    /// Gets the singleton instance of AppConfiguration
    /// </summary>
    public static AppConfiguration Instance => _lazyInstance.Value;

    /// <summary>
    /// Loads configuration from appsettings.json
    /// </summary>
    private static AppConfiguration LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();
        var appConfig = new AppConfiguration();
        configuration.Bind(appConfig);

        return appConfig;
    }
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
    public int ErrorQueueProcessingDelayMs { get; set; } = AppConstants.ErrorQueueProcessingDelayMs;
}

/// <summary>
/// UI configuration settings
/// </summary>
public class UIConfiguration
{
    public int DashboardRecentInvoices { get; set; } = AppConstants.DashboardRecentInvoicesCount;
    public int PageSize { get; set; } = AppConstants.DefaultPageSize;
    public int SearchDebounceMs { get; set; } = 300;
}

/// <summary>
/// Business rules configuration
/// </summary>
public class BusinessConfiguration
{
    public decimal DefaultGstRate { get; set; } = AppConstants.DefaultGstRate;
    public int DefaultPaymentTermsDays { get; set; } = AppConstants.DefaultPaymentTermsDays;
    public string InvoiceNumberPrefix { get; set; } = "INV";
    public string Currency { get; set; } = "AUD";
}

/// <summary>
/// Performance configuration settings
/// </summary>
public class PerformanceConfiguration
{
    public int SettingsCacheExpirationMinutes { get; set; } = 5;
    public int MaxConcurrentDatabaseConnections { get; set; } = 10;
}

/// <summary>
/// Backup configuration settings
/// </summary>
public class BackupConfiguration
{
    public bool AutoBackupEnabled { get; set; } = false;
    public int AutoBackupIntervalDays { get; set; } = 7;
    public int MaxBackupRetentionDays { get; set; } = 90;
}

/// <summary>
/// Template configuration settings
/// </summary>
public class TemplatesConfiguration
{
    public string InvoiceTemplate { get; set; } = "Default";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string NumberFormat { get; set; } = "N2";
}
