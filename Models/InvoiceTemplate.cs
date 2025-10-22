using System.ComponentModel.DataAnnotations;
using AuroraInvoice.Common;

namespace AuroraInvoice.Models;

/// <summary>
/// Customizable invoice template settings
/// </summary>
public class InvoiceTemplate
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string TemplateName { get; set; } = "Default";

    // Header Fields
    public bool ShowBusinessLogo { get; set; } = false;
    public bool ShowBusinessAddress { get; set; } = true;
    public bool ShowBusinessPhone { get; set; } = true;
    public bool ShowBusinessEmail { get; set; } = true;
    public bool ShowABN { get; set; } = true;

    // Invoice Details
    public bool ShowInvoiceNumber { get; set; } = true;
    public bool ShowInvoiceDate { get; set; } = true;
    public bool ShowDueDate { get; set; } = false; // You don't need this
    public bool ShowServiceDate { get; set; } = false; // For invoice-level service date

    // Customer Fields
    public bool ShowCustomerName { get; set; } = true;
    public bool ShowCustomerContactPerson { get; set; } = true;
    public bool ShowCustomerAddress { get; set; } = true;
    public bool ShowCustomerPhone { get; set; } = true;
    public bool ShowCustomerEmail { get; set; } = true;
    public bool ShowCustomerABN { get; set; } = false;

    // Line Item Columns
    public bool ShowItemNumber { get; set; } = true;
    public bool ShowItemDescription { get; set; } = true;
    public bool ShowItemServiceDate { get; set; } = true; // Service date per line item
    public bool ShowItemQuantity { get; set; } = true;
    public bool ShowItemUnitPrice { get; set; } = true;
    public bool ShowItemGST { get; set; } = false; // Show GST per line
    public bool ShowItemTotal { get; set; } = true;

    // Column Labels (customizable)
    [MaxLength(50)]
    public string ItemNumberLabel { get; set; } = "#";

    [MaxLength(50)]
    public string ItemDescriptionLabel { get; set; } = "Item & Description";

    [MaxLength(50)]
    public string ItemServiceDateLabel { get; set; } = "Service Date";

    [MaxLength(50)]
    public string ItemQuantityLabel { get; set; } = "Hrs";

    [MaxLength(50)]
    public string ItemUnitPriceLabel { get; set; } = "Rate";

    [MaxLength(50)]
    public string ItemTotalLabel { get; set; } = "Amount";

    // Totals Section
    public bool ShowSubtotal { get; set; } = false; // You may not need this
    public bool ShowGSTTotal { get; set; } = false; // You may not need this
    public bool ShowGrandTotal { get; set; } = true;

    // Footer
    public bool ShowPaymentTerms { get; set; } = false;
    public bool ShowNotes { get; set; } = true;
    public bool ShowThankYouMessage { get; set; } = false;

    [MaxLength(500)]
    public string? CustomFooterText { get; set; }

    // Styling
    [MaxLength(20)]
    public string HeaderColor { get; set; } = "#008080"; // Teal

    [MaxLength(20)]
    public string AccentColor { get; set; } = "#008080"; // Teal

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTimeProvider.UtcNow;
    public DateTime? ModifiedDate { get; set; }
}
