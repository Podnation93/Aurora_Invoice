using System.ComponentModel.DataAnnotations;

namespace AuroraInvoice.Models;

public class ErrorLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(50)]
    public string Severity { get; set; } = "Error"; // Error, Warning, Info, Critical

    [Required]
    [MaxLength(200)]
    public string Source { get; set; } = string.Empty; // Class/method name

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public string? AdditionalInfo { get; set; }

    [MaxLength(100)]
    public string? UserAction { get; set; } // What the user was doing when error occurred

    public bool IsResolved { get; set; } = false;
}
