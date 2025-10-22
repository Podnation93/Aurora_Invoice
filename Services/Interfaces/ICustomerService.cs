using AuroraInvoice.Models;

namespace AuroraInvoice.Services.Interfaces;

/// <summary>
/// Service interface for customer management operations
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Gets all customers with optional pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of customers</returns>
    Task<(List<Customer> Customers, int TotalCount)> GetCustomersAsync(int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Searches customers by text query with pagination
    /// </summary>
    /// <param name="searchText">Search query</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated search results</returns>
    Task<(List<Customer> Customers, int TotalCount)> SearchCustomersAsync(string searchText, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer or null if not found</returns>
    Task<Customer?> GetCustomerByIdAsync(int customerId);

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customer">Customer to create</param>
    /// <returns>Created customer with ID</returns>
    Task<Customer> CreateCustomerAsync(Customer customer);

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="customer">Customer to update</param>
    Task UpdateCustomerAsync(Customer customer);

    /// <summary>
    /// Deletes a customer if they have no invoices
    /// </summary>
    /// <param name="customerId">Customer ID to delete</param>
    /// <returns>True if deleted, false if customer has invoices</returns>
    /// <exception cref="InvalidOperationException">Thrown when customer has associated invoices</exception>
    Task<bool> DeleteCustomerAsync(int customerId);

    /// <summary>
    /// Checks if a customer has any invoices
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Number of invoices for the customer</returns>
    Task<int> GetInvoiceCountAsync(int customerId);
}
