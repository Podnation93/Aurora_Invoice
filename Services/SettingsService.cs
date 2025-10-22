using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for managing application settings with caching
/// </summary>
public class SettingsService : ISettingsService
{
    private AppSettings? _cachedSettings;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTime _cacheTime = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the current application settings with caching
    /// </summary>
    /// <returns>Application settings</returns>
    /// <exception cref="InvalidOperationException">Thrown when settings are not initialized</exception>
    public async Task<AppSettings> GetSettingsAsync()
    {
        // Check if cache is valid
        if (_cachedSettings != null && DateTime.UtcNow - _cacheTime < _cacheExpiration)
            return _cachedSettings;

        await _lock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedSettings != null && DateTime.UtcNow - _cacheTime < _cacheExpiration)
                return _cachedSettings;

            using var context = new AuroraDbContext();
            _cachedSettings = await context.AppSettings.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Application settings not initialized. Database may be corrupted.");

            _cacheTime = DateTime.UtcNow;
            return _cachedSettings;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Updates application settings and invalidates cache
    /// </summary>
    /// <param name="settings">Settings to update</param>
    public async Task UpdateSettingsAsync(AppSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        await _lock.WaitAsync();
        try
        {
            using var context = new AuroraDbContext();

            settings.ModifiedDate = DateTimeProvider.UtcNow;
            context.AppSettings.Update(settings);
            await context.SaveChangesAsync();

            // Update cache
            _cachedSettings = settings;
            _cacheTime = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Invalidates the settings cache, forcing a reload on next access
    /// </summary>
    public void InvalidateCache()
    {
        _cachedSettings = null;
        _cacheTime = DateTime.MinValue;
    }

    /// <summary>
    /// Gets the current GST rate from settings
    /// </summary>
    /// <returns>GST rate as decimal (e.g., 0.10 for 10%)</returns>
    public async Task<decimal> GetGstRateAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.DefaultGSTRate;
    }
}
