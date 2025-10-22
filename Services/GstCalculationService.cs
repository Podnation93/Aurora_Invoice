using AuroraInvoice.Services.Interfaces;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for calculating GST (Goods and Services Tax) for invoices and expenses
/// </summary>
public class GstCalculationService
{
    private readonly ISettingsService _settingsService;

    public GstCalculationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    // Async methods that use SettingsService for GST rate

    /// <summary>
    /// Calculate GST amount from an amount that includes GST (uses settings for GST rate)
    /// </summary>
    /// <param name="totalAmount">Total amount including GST</param>
    /// <returns>GST component of the total amount</returns>
    public async Task<decimal> CalculateGstFromTotalAsync(decimal totalAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        return CalculateGstFromTotal(totalAmount, gstRate);
    }

    /// <summary>
    /// Calculate GST amount to add to a base amount (uses settings for GST rate)
    /// </summary>
    /// <param name="baseAmount">Amount before GST</param>
    /// <returns>GST amount to add</returns>
    public async Task<decimal> CalculateGstToAddAsync(decimal baseAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        return CalculateGstToAdd(baseAmount, gstRate);
    }

    /// <summary>
    /// Calculate total amount including GST (uses settings for GST rate)
    /// </summary>
    /// <param name="baseAmount">Amount before GST</param>
    /// <returns>Total amount including GST</returns>
    public async Task<decimal> CalculateTotalWithGstAsync(decimal baseAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        return CalculateTotalWithGst(baseAmount, gstRate);
    }

    /// <summary>
    /// Calculate base amount excluding GST from a total that includes GST (uses settings for GST rate)
    /// </summary>
    /// <param name="totalAmount">Total amount including GST</param>
    /// <returns>Base amount excluding GST</returns>
    public async Task<decimal> CalculateBaseFromTotalAsync(decimal totalAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        return CalculateBaseFromTotal(totalAmount, gstRate);
    }

    // Synchronous methods with explicit GST rate (for backward compatibility)

    /// <summary>
    /// Calculate GST amount from an amount that includes GST
    /// </summary>
    /// <param name="totalAmount">Total amount including GST</param>
    /// <param name="gstRate">GST rate (e.g., 0.10 for 10%)</param>
    /// <returns>GST component of the total amount</returns>
    public decimal CalculateGstFromTotal(decimal totalAmount, decimal gstRate)
    {
        if (gstRate <= 0) return 0;

        // Formula: GST = Total × (GST Rate / (1 + GST Rate))
        return Math.Round(totalAmount * (gstRate / (1 + gstRate)), 2);
    }

    /// <summary>
    /// Calculate GST amount to add to a base amount
    /// </summary>
    /// <param name="baseAmount">Amount before GST</param>
    /// <param name="gstRate">GST rate (e.g., 0.10 for 10%)</param>
    /// <returns>GST amount to add</returns>
    public decimal CalculateGstToAdd(decimal baseAmount, decimal gstRate)
    {
        if (gstRate <= 0) return 0;

        return Math.Round(baseAmount * gstRate, 2);
    }

    /// <summary>
    /// Calculate total amount including GST
    /// </summary>
    /// <param name="baseAmount">Amount before GST</param>
    /// <param name="gstRate">GST rate (e.g., 0.10 for 10%)</param>
    /// <returns>Total amount including GST</returns>
    public decimal CalculateTotalWithGst(decimal baseAmount, decimal gstRate)
    {
        var gstAmount = CalculateGstToAdd(baseAmount, gstRate);
        return baseAmount + gstAmount;
    }

    /// <summary>
    /// Calculate base amount excluding GST from a total that includes GST
    /// </summary>
    /// <param name="totalAmount">Total amount including GST</param>
    /// <param name="gstRate">GST rate (e.g., 0.10 for 10%)</param>
    /// <returns>Base amount excluding GST</returns>
    public decimal CalculateBaseFromTotal(decimal totalAmount, decimal gstRate)
    {
        if (gstRate <= 0) return totalAmount;

        return Math.Round(totalAmount / (1 + gstRate), 2);
    }
}
