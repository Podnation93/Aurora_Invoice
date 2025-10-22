namespace AuroraInvoice.Common;

/// <summary>
/// Provides DateTime values using UTC to avoid timezone and DST issues
/// </summary>
public static class DateTimeProvider
{
    /// <summary>
    /// Gets the current date and time in UTC
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Converts UTC DateTime to local time for display purposes
    /// </summary>
    /// <param name="utcDateTime">UTC DateTime to convert</param>
    /// <returns>DateTime in local timezone</returns>
    public static DateTime ToLocalTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Utc)
            return utcDateTime.ToLocalTime();

        // If already local or unspecified, return as-is
        return utcDateTime;
    }

    /// <summary>
    /// Converts local DateTime to UTC for storage
    /// </summary>
    /// <param name="localDateTime">Local DateTime to convert</param>
    /// <returns>DateTime in UTC</returns>
    public static DateTime ToUtcTime(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc)
            return localDateTime;

        if (localDateTime.Kind == DateTimeKind.Local)
            return localDateTime.ToUniversalTime();

        // Assume unspecified is local
        return DateTime.SpecifyKind(localDateTime, DateTimeKind.Local).ToUniversalTime();
    }

    /// <summary>
    /// Gets the current date (without time) in UTC
    /// </summary>
    public static DateTime UtcToday => DateTime.UtcNow.Date;

    /// <summary>
    /// Gets the current date (without time) in local timezone
    /// </summary>
    public static DateTime Today => DateTime.Today;
}
