using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuroraInvoice.Models;

/// <summary>
/// Represents a line item on an invoice
/// </summary>
public class InvoiceItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public Invoice Invoice { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Service date for this line item (optional)
    /// </summary>
    public DateTime? ServiceDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// GST rate as a decimal (e.g., 0.10 for 10%, 0 for GST-free)
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal GSTRate { get; set; } = 0.10m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GSTAmount { get; set; }
}
