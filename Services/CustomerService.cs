using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Common;

namespace AuroraInvoice.Services;

/// <summary>
/// Service for customer management operations
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IAuditService _auditService;

    public CustomerService(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Gets all customers with optional pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of customers and total count</returns>
    public async Task<(List<Customer> Customers, int TotalCount)> GetCustomersAsync(int pageNumber = 1, int pageSize = 50)
    {
        using var context = new AuroraDbContext();

        var totalCount = await context.Customers.CountAsync();

        var customers = await context.Customers
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (customers, totalCount);
    }

    /// <summary>
    /// Searches customers by text query with pagination
    /// </summary>
    /// <param name="searchText">Search query</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated search results</returns>
    public async Task<(List<Customer> Customers, int TotalCount)> SearchCustomersAsync(string searchText, int pageNumber = 1, int pageSize = 50)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return await GetCustomersAsync(pageNumber, pageSize);

        // Sanitize search text
        searchText = new string(searchText
            .Trim()
            .Take(AppConstants.SearchTextMaxLength)
            .Where(c => !char.IsControl(c))
            .ToArray());

        using var context = new AuroraDbContext();

        var query = context.Customers.Where(c =>
            c.Name.Contains(searchText) ||
            (c.ContactPerson != null && c.ContactPerson.Contains(searchText)) ||
            (c.Email != null && c.Email.Contains(searchText)) ||
            (c.ABN != null && c.ABN.Contains(searchText)));

        var totalCount = await query.CountAsync();

        var customers = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (customers, totalCount);
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer or null if not found</returns>
    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        using var context = new AuroraDbContext();
        return await context.Customers.FindAsync(customerId);
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customer">Customer to create</param>
    /// <returns>Created customer with ID</returns>
    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        using var context = new AuroraDbContext();

        customer.CreatedDate = DateTimeProvider.UtcNow;
        customer.ModifiedDate = null;

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAuditAsync("Create", "Customer", customer.Id, $"Created customer: {customer.Name}");

        return customer;
    }

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="customer">Customer to update</param>
    public async Task UpdateCustomerAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        using var context = new AuroraDbContext();

        var existing = await context.Customers.FindAsync(customer.Id);
        if (existing == null)
            throw new InvalidOperationException($"Customer with ID {customer.Id} not found");

        // Update properties
        existing.Name = customer.Name;
        existing.ContactPerson = customer.ContactPerson;
        existing.Address = customer.Address;
        existing.Phone = customer.Phone;
        existing.Email = customer.Email;
        existing.ABN = customer.ABN;
        existing.ModifiedDate = DateTimeProvider.UtcNow;

        await context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAuditAsync("Update", "Customer", customer.Id, $"Updated customer: {customer.Name}");
    }

    /// <summary>
    /// Deletes a customer if they have no invoices
    /// </summary>
    /// <param name="customerId">Customer ID to delete</param>
    /// <returns>True if deleted, false if customer has invoices</returns>
    /// <exception cref="InvalidOperationException">Thrown when customer has associated invoices</exception>
    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        using var context = new AuroraDbContext();

        var customer = await context.Customers.FindAsync(customerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer with ID {customerId} not found");

        // Check for invoices
        var invoiceCount = await context.Invoices
            .Where(i => i.CustomerId == customerId)
            .CountAsync();

        if (invoiceCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete customer '{customer.Name}' because {invoiceCount} invoice(s) are associated with this customer. " +
                "Please delete or reassign the invoices first.");
        }

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();

        // Log audit
        await _auditService.LogAuditAsync("Delete", "Customer", customerId, $"Deleted customer: {customer.Name}");

        return true;
    }

    /// <summary>
    /// Checks if a customer has any invoices
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Number of invoices for the customer</returns>
    public async Task<int> GetInvoiceCountAsync(int customerId)
    {
        using var context = new AuroraDbContext();
        return await context.Invoices
            .Where(i => i.CustomerId == customerId)
            .CountAsync();
    }
}
