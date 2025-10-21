using System.IO;
using System.IO.Compression;
using AuroraInvoice.Data;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for backing up and restoring the application database
/// </summary>
public class BackupService
{
    private readonly DatabaseService _databaseService;

    public BackupService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Create a backup of the database to the specified folder
    /// </summary>
    /// <param name="backupFolderPath">Folder path to save the backup</param>
    /// <returns>Path to the created backup file</returns>
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

            // Create backup filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"aurora_invoice_backup_{timestamp}.db";
            var backupFilePath = Path.Combine(backupFolderPath, backupFileName);

            // Copy database file
            await Task.Run(() => File.Copy(dbPath, backupFilePath, overwrite: true));

            // Optionally compress the backup
            var compressedBackupPath = backupFilePath + ".zip";
            await Task.Run(() =>
            {
                using var zip = ZipFile.Open(compressedBackupPath, ZipArchiveMode.Create);
                zip.CreateEntryFromFile(backupFilePath, backupFileName);
            });

            // Delete uncompressed backup
            File.Delete(backupFilePath);

            return compressedBackupPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create backup", ex);
        }
    }

    /// <summary>
    /// Restore database from a backup file
    /// </summary>
    /// <param name="backupFilePath">Path to the backup file</param>
    public async Task RestoreBackupAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
            {
                throw new FileNotFoundException("Backup file not found", backupFilePath);
            }

            var dbPath = _databaseService.GetDatabasePath();
            var tempExtractPath = Path.Combine(Path.GetTempPath(), "AuroraInvoiceRestore");

            // Clean temp directory if it exists
            if (Directory.Exists(tempExtractPath))
            {
                Directory.Delete(tempExtractPath, recursive: true);
            }
            Directory.CreateDirectory(tempExtractPath);

            // Extract backup
            await Task.Run(() => ZipFile.ExtractToDirectory(backupFilePath, tempExtractPath));

            // Find the .db file in extracted contents
            var extractedDbFile = Directory.GetFiles(tempExtractPath, "*.db").FirstOrDefault();

            if (extractedDbFile == null)
            {
                throw new InvalidOperationException("No database file found in backup");
            }

            // Create backup of current database before restoring
            var currentDbBackupPath = dbPath + ".pre_restore_backup";
            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, currentDbBackupPath, overwrite: true);
            }

            // Restore the database
            await Task.Run(() => File.Copy(extractedDbFile, dbPath, overwrite: true));

            // Clean up temp directory
            Directory.Delete(tempExtractPath, recursive: true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to restore backup", ex);
        }
    }

    /// <summary>
    /// Get list of available backups in a folder
    /// </summary>
    /// <param name="backupFolderPath">Folder to search for backups</param>
    /// <returns>List of backup file paths</returns>
    public List<string> GetAvailableBackups(string backupFolderPath)
    {
        if (!Directory.Exists(backupFolderPath))
        {
            return new List<string>();
        }

        return Directory.GetFiles(backupFolderPath, "aurora_invoice_backup_*.zip")
            .OrderByDescending(f => f)
            .ToList();
    }
}
