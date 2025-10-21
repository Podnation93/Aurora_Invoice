using System.ComponentModel.DataAnnotations;

namespace AuroraInvoice.Models;

/// <summary>
/// Represents a customer/client in the system
/// </summary>
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ContactPerson { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? ABN { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? ModifiedDate { get; set; }

    // Navigation property
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
