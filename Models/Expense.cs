using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuroraInvoice.Common;

namespace AuroraInvoice.Models;

/// <summary>
/// Represents a business expense
/// </summary>
public class Expense
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTimeProvider.UtcNow;

    [Required]
    [MaxLength(200)]
    public string Vendor { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// GST component included in the amount (input tax credit)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal GSTAmount { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ExpenseCategory Category { get; set; } = null!;

    /// <summary>
    /// Path to attached receipt/document (optional)
    /// </summary>
    [MaxLength(1000)]
    public string? ReceiptPath { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTimeProvider.UtcNow;

    public DateTime? ModifiedDate { get; set; }
}
