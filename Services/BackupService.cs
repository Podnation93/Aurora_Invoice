using System.IO;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Common;
using AuroraInvoice.Services.Interfaces;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for backing up and restoring the application database with safety validation
/// </summary>
public class BackupService
{
    private readonly DatabaseService _databaseService;
    private readonly IAuditService? _auditService;

    public BackupService(DatabaseService databaseService, IAuditService? auditService = null)
    {
        _databaseService = databaseService;
        _auditService = auditService;
    }

    /// <summary>
    /// Create a backup of the database to the specified folder
    /// </summary>
    /// <param name="backupFolderPath">Folder path to save the backup</param>
    /// <returns>Path to the created backup file</returns>
    /// <exception cref="FileNotFoundException">Thrown when database file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when backup operation fails</exception>
    public async Task<string> CreateBackupAsync(string backupFolderPath)
    {
        try
        {
            // Ensure backup folder exists
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
            }

            var dbPath = _databaseService.GetDatabasePath();

            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Database file not found", dbPath);
            }

            // Create backup filename with timestamp (use UTC)
            var timestamp = DateTimeProvider.UtcNow.ToString(AppConstants.BackupTimestampFormat);
            var backupFileName = $"aurora_invoice_backup_{timestamp}.db";
            var backupFilePath = Path.Combine(backupFolderPath, backupFileName);

            // Copy database file
            await Task.Run(() => File.Copy(dbPath, backupFilePath, overwrite: true));

            // Compress the backup
            var compressedBackupPath = backupFilePath + ".zip";
            await Task.Run(() =>
            {
                using var zip = ZipFile.Open(compressedBackupPath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(backupFilePath, backupFileName);
            });

            // Delete uncompressed backup
            File.Delete(backupFilePath);

            // Log audit
            if (_auditService != null)
            {
                await _auditService.LogAuditAsync("Backup", "Database", 0,
                    $"Created backup: {Path.GetFileName(compressedBackupPath)}");
            }

            return compressedBackupPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create backup", ex);
        }
    }

    /// <summary>
    /// Restore database from a backup file with validation and rollback support
    /// </summary>
    /// <param name="backupFilePath">Path to the backup file</param>
    /// <exception cref="FileNotFoundException">Thrown when backup file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when restore operation fails</exception>
    public async Task RestoreBackupAsync(string backupFilePath)
    {
        var dbPath = _databaseService.GetDatabasePath();
        var safetyBackupPath = dbPath + AppConstants.PreRestoreBackupSuffix + "_" +
            DateTimeProvider.UtcNow.ToString(AppConstants.BackupTimestampFormat);
        var tempExtractPath = Path.Combine(Path.GetTempPath(), "AuroraInvoiceRestore_" + Guid.NewGuid());

        try
        {
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException("Backup file not found", backupFilePath);
            }

            // Create safety backup of current database
            if (File.Exists(dbPath))
            {
                await Task.Run(() => File.Copy(dbPath, safetyBackupPath, overwrite: true));
            }

            // Create temp extraction directory
            Directory.CreateDirectory(tempExtractPath);

            // Extract backup
            await Task.Run(() => ZipFile.ExtractToDirectory(backupFilePath, tempExtractPath));

            // Find the .db file in extracted contents
            var extractedDbFile = Directory.GetFiles(tempExtractPath, "*.db").FirstOrDefault();

            if (extractedDbFile == null)
            {
                throw new InvalidOperationException("No database file found in backup archive");
            }

            // Validate extracted database before overwriting
            var isValid = await ValidateDatabaseFileAsync(extractedDbFile);
            if (!isValid)
            {
                throw new InvalidOperationException("Backup file is corrupted or invalid. Restore aborted.");
            }

            // Restore the database
            await Task.Run(() => File.Copy(extractedDbFile, dbPath, overwrite: true));

            // Verify restored database
            var restoredIsValid = await ValidateDatabaseFileAsync(dbPath);
            if (!restoredIsValid)
            {
                // Rollback: restore from safety backup
                if (File.Exists(safetyBackupPath))
                {
                    await Task.Run(() => File.Copy(safetyBackupPath, dbPath, overwrite: true));
                }
                throw new InvalidOperationException("Restored database validation failed. Original database has been restored.");
            }

            // Clean up temp directory
            Directory.Delete(tempExtractPath, recursive: true);

            // Only delete safety backup if restore was successful
            if (File.Exists(safetyBackupPath))
            {
                File.Delete(safetyBackupPath);
            }

            // Log audit
            if (_auditService != null)
            {
                await _auditService.LogAuditAsync("Restore", "Database", 0,
                    $"Restored from backup: {Path.GetFileName(backupFilePath)}");
            }
        }
        catch (Exception ex)
        {
            // Attempt to restore from safety backup on any error
            if (File.Exists(safetyBackupPath) && File.Exists(dbPath))
            {
                try
                {
                    await Task.Run(() => File.Copy(safetyBackupPath, dbPath, overwrite: true));
                }
                catch
                {
                    // If rollback fails, leave safety backup in place for manual recovery
                }
            }

            // Clean up temp directory if it exists
            if (Directory.Exists(tempExtractPath))
            {
                try
                {
                    Directory.Delete(tempExtractPath, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            throw new InvalidOperationException($"Failed to restore backup: {ex.Message}. " +
                $"If database is corrupted, a safety backup is available at: {safetyBackupPath}", ex);
        }
    }

    /// <summary>
    /// Validates a SQLite database file by attempting to open it and query basic structure
    /// </summary>
    /// <param name="databasePath">Path to database file to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private async Task<bool> ValidateDatabaseFileAsync(string databasePath)
    {
        try
        {
            // Create a temporary context with the specified database path
            var optionsBuilder = new DbContextOptionsBuilder<AuroraDbContext>();
            optionsBuilder.UseSqlite($"Data Source={databasePath}");

            using var context = new AuroraDbContext(optionsBuilder.Options);

            // Try to connect and query
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
                return false;

            // Try to query a basic table to ensure structure is valid
            var hasSettings = await context.AppSettings.AnyAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get list of available backups in a folder
    /// </summary>
    /// <param name="backupFolderPath">Folder to search for backups</param>
    /// <returns>List of backup file paths ordered by date (newest first)</returns>
    public List<string> GetAvailableBackups(string backupFolderPath)
    {
        if (!Directory.Exists(backupFolderPath))
        {
            return new List<string>();
        }

        return Directory.GetFiles(backupFolderPath, AppConstants.BackupFilePattern)
            .OrderByDescending(f => f)
            .ToList();
    }
}
