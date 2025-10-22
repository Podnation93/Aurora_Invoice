using System.ComponentModel.DataAnnotations;
using AuroraInvoice.Common;

namespace AuroraInvoice.Models;

/// <summary>
/// Represents a category for organizing expenses
/// </summary>
public class ExpenseCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTimeProvider.UtcNow;

    // Navigation property
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
