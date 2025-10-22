using AuroraInvoice.Models;

namespace AuroraInvoice.Services.Interfaces;

/// <summary>
/// Service interface for managing application settings
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current application settings with caching
    /// </summary>
    /// <returns>Application settings</returns>
    Task<AppSettings> GetSettingsAsync();

    /// <summary>
    /// Updates application settings and invalidates cache
    /// </summary>
    /// <param name="settings">Settings to update</param>
    Task UpdateSettingsAsync(AppSettings settings);

    /// <summary>
    /// Invalidates the settings cache, forcing a reload on next access
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Gets the current GST rate from settings
    /// </summary>
    Task<decimal> GetGstRateAsync();
}
