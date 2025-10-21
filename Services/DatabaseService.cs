using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for managing database operations and initialization
/// </summary>
public class DatabaseService
{
    private readonly AuroraDbContext _context;

    public DatabaseService(AuroraDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Initialize the database, applying any pending migrations
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // Apply any pending migrations
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize database", ex);
        }
    }

    /// <summary>
    /// Check if the database exists and is accessible
    /// </summary>
    public async Task<bool> IsDatabaseAccessibleAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the database file path
    /// </summary>
    public string GetDatabasePath()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AuroraInvoice",
            "aurora_invoice.db");

        return dbPath;
    }
}
