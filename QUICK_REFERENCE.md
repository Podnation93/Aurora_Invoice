# Aurora Invoice - Quick Reference Guide

## 🚀 Getting Started

### Build & Run
```bash
cd E:\Programming\Aurora_Invoice
dotnet restore
dotnet build
dotnet run
```

### Apply Latest Database Migration
```bash
dotnet ef migrations add AddAuditLogTable
dotnet ef database update
```

---

## 📁 Project Structure

```
Aurora_Invoice/
├── Common/                    # NEW - Utilities & Constants
│   ├── AppConstants.cs       # All constants (no magic numbers!)
│   ├── DateTimeProvider.cs   # Use instead of DateTime.Now
│   └── AppConfiguration.cs   # Configuration classes
│
├── Data/
│   └── AuroraDbContext.cs    # Database context
│
├── Models/                    # Entities
│   ├── Customer.cs
│   ├── Invoice.cs
│   ├── InvoiceItem.cs
│   ├── Expense.cs
│   ├── ExpenseCategory.cs
│   ├── AppSettings.cs
│   ├── ErrorLog.cs
│   ├── InvoiceTemplate.cs
│   └── AuditLog.cs           # NEW - Audit trail
│
├── Services/                  # Business logic
│   ├── Interfaces/           # NEW - Service contracts
│   │   ├── ISettingsService.cs
│   │   ├── IAuditService.cs
│   │   └── ICustomerService.cs
│   ├── SettingsService.cs    # NEW - Settings with caching
│   ├── AuditService.cs       # NEW - Audit logging
│   ├── CustomerService.cs    # NEW - Customer operations
│   ├── BackupService.cs      # IMPROVED - Safe restore
│   ├── DatabaseService.cs
│   ├── GstCalculationService.cs
│   ├── InvoicePdfService.cs
│   └── LoggingService.cs
│
├── Views/                     # UI Pages
│   ├── DashboardPage.xaml(.cs)
│   ├── CustomersPage.xaml(.cs)
│   ├── InvoicesPage.xaml(.cs)
│   └── ... (other pages)
│
└── Controls/                  # Custom controls
    └── SimpleBarChart.xaml(.cs)
```

---

## 🔧 Common Patterns

### ✅ Creating a Service

```csharp
// 1. Define interface in Services/Interfaces/
public interface IMyService
{
    Task<Something> DoSomethingAsync();
}

// 2. Implement in Services/
public class MyService : IMyService
{
    private readonly IAuditService _auditService;

    public MyService(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<Something> DoSomethingAsync()
    {
        using var context = new AuroraDbContext();
        // ... do work ...

        // Log audit trail
        await _auditService.LogAuditAsync("Create", "EntityType", entityId);

        return result;
    }
}
```

### ✅ Using DateTimeProvider

```csharp
// ❌ WRONG:
customer.CreatedDate = DateTime.Now;

// ✅ CORRECT:
using AuroraInvoice.Common;
customer.CreatedDate = DateTimeProvider.UtcNow;

// For display purposes:
var localTime = DateTimeProvider.ToLocalTime(customer.CreatedDate);
```

### ✅ Using AppConstants

```csharp
// ❌ WRONG:
var pageSize = 50;
var dbName = "aurora_invoice.db";
var gstRate = 0.10m;

// ✅ CORRECT:
using AuroraInvoice.Common;
var pageSize = AppConstants.DefaultPageSize;
var dbName = AppConstants.DatabaseFileName;
var gstRate = AppConstants.DefaultGstRate;
```

### ✅ Using SettingsService

```csharp
var settingsService = new SettingsService();

// Get settings (cached)
var settings = await settingsService.GetSettingsAsync();

// Get just GST rate
var gstRate = await settingsService.GetGstRateAsync();

// Update settings (invalidates cache)
settings.DefaultGSTRate = 0.15m;
await settingsService.UpdateSettingsAsync(settings);
```

### ✅ Using AuditService

```csharp
var auditService = new AuditService();

// Log successful operation
await auditService.LogAuditAsync(
    action: "Create",
    entityType: "Invoice",
    entityId: invoice.Id,
    details: $"Invoice #{invoice.InvoiceNumber}"
);

// Get recent audits
var audits = await auditService.GetRecentAuditsAsync(count: 50);

// Get audits for specific entity
var invoiceAudits = await auditService.GetRecentAuditsAsync(
    count: 100,
    entityType: "Invoice"
);
```

### ✅ Using CustomerService

```csharp
var auditService = new AuditService();
var customerService = new CustomerService(auditService);

// Get paginated customers
var (customers, totalCount) = await customerService.GetCustomersAsync(
    pageNumber: 1,
    pageSize: AppConstants.DefaultPageSize
);

// Search customers
var (results, count) = await customerService.SearchCustomersAsync(
    searchText: "John",
    pageNumber: 1,
    pageSize: 50
);

// Create customer
var newCustomer = new Customer { Name = "Acme Corp" };
var created = await customerService.CreateCustomerAsync(newCustomer);

// Update customer
customer.Name = "Acme Corporation";
await customerService.UpdateCustomerAsync(customer);

// Delete customer (checks for invoices)
try
{
    await customerService.DeleteCustomerAsync(customerId);
}
catch (InvalidOperationException ex)
{
    // Customer has invoices, can't delete
    MessageBox.Show(ex.Message);
}
```

### ✅ Using BackupService (Improved)

```csharp
var databaseService = new DatabaseService(context);
var auditService = new AuditService();
var backupService = new BackupService(databaseService, auditService);

// Create backup
var backupPath = await backupService.CreateBackupAsync(@"C:\Backups");
// Result: C:\Backups\aurora_invoice_backup_20251022_143022.db.zip

// Restore backup (with validation and automatic rollback)
try
{
    await backupService.RestoreBackupAsync(backupPath);
    // If validation fails, automatically restores original database
}
catch (InvalidOperationException ex)
{
    // Restore failed, but original DB is safe
    MessageBox.Show(ex.Message);
}

// Get available backups
var backups = backupService.GetAvailableBackups(@"C:\Backups");
```

### ✅ Proper Error Handling

```csharp
private async void Button_Click(object sender, RoutedEventArgs e)
{
    try
    {
        await DoSomethingAsync();
    }
    catch (Exception ex)
    {
        await LoggingService.LogErrorAsync(ex, "ClassName.MethodName");
        MessageBox.Show($"Error: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

### ✅ Using Transactions

```csharp
using var context = new AuroraDbContext();
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    // Do multiple operations
    context.Invoices.Add(invoice);
    settings.NextInvoiceNumber++;

    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 📋 Database Cheat Sheet

### Connection String
```
Data Source=%LocalAppData%\AuroraInvoice\aurora_invoice.db;Cache=Shared;Mode=ReadWriteCreate
```

### Create Migration
```bash
dotnet ef migrations add MigrationName
```

### Apply Migrations
```bash
dotnet ef database update
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove
```

---

## 🔍 Debugging Tips

### Check if Settings are Cached
```csharp
var settingsService = new SettingsService();
var settings1 = await settingsService.GetSettingsAsync(); // DB query
var settings2 = await settingsService.GetSettingsAsync(); // From cache (fast)
```

### View Audit Logs
```csharp
var auditService = new AuditService();
var audits = await auditService.GetRecentAuditsAsync(100);

foreach (var audit in audits)
{
    Debug.WriteLine($"[{audit.Timestamp}] {audit.Action} {audit.EntityType} #{audit.EntityId} by {audit.UserName}");
}
```

### Check Database Connection
```csharp
var databaseService = new DatabaseService(context);
var isAccessible = await databaseService.IsDatabaseAccessibleAsync();
var dbPath = databaseService.GetDatabasePath();
Debug.WriteLine($"DB accessible: {isAccessible} at {dbPath}");
```

---

## ⚠️ Common Mistakes to Avoid

### ❌ DON'T: Use DateTime.Now
```csharp
customer.CreatedDate = DateTime.Now;  // Causes DST bugs!
```

### ✅ DO: Use DateTimeProvider
```csharp
customer.CreatedDate = DateTimeProvider.UtcNow;
```

---

### ❌ DON'T: Hard-code magic numbers
```csharp
var customers = await context.Customers.Take(50).ToListAsync();
var gst = amount * 0.10m;
```

### ✅ DO: Use AppConstants and SettingsService
```csharp
var customers = await context.Customers.Take(AppConstants.DefaultPageSize).ToListAsync();
var gstRate = await settingsService.GetGstRateAsync();
var gst = amount * gstRate;
```

---

### ❌ DON'T: Direct DB access in Views
```csharp
// In CustomersPage.xaml.cs
using var context = new AuroraDbContext();
var customers = await context.Customers.ToListAsync();
```

### ✅ DO: Use Service Layer
```csharp
var customerService = new CustomerService(auditService);
var (customers, totalCount) = await customerService.GetCustomersAsync();
```

---

### ❌ DON'T: Forget to log audits
```csharp
context.Customers.Add(customer);
await context.SaveChangesAsync();
```

### ✅ DO: Log all business operations
```csharp
await customerService.CreateCustomerAsync(customer);
// Service automatically logs audit trail
```

---

### ❌ DON'T: Ignore transaction safety
```csharp
context.Invoices.Add(invoice);
context.InvoiceItems.AddRange(items);
settings.NextInvoiceNumber++;
await context.SaveChangesAsync();  // If this fails partway through, data is inconsistent!
```

### ✅ DO: Use transactions
```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    context.Invoices.Add(invoice);
    context.InvoiceItems.AddRange(items);
    settings.NextInvoiceNumber++;
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 📊 Performance Tips

### 1. Use Pagination
```csharp
// ❌ BAD: Loads ALL customers into memory
var customers = await context.Customers.ToListAsync();

// ✅ GOOD: Loads only what's needed
var customers = await context.Customers
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 2. Use Projections
```csharp
// ❌ BAD: Loads entire entities
var invoices = await context.Invoices.Include(i => i.Customer).ToListAsync();

// ✅ GOOD: Only loads needed fields
var invoiceSummaries = await context.Invoices
    .Select(i => new {
        i.InvoiceNumber,
        CustomerName = i.Customer.Name,
        i.TotalAmount
    })
    .ToListAsync();
```

### 3. Cache Settings
```csharp
// ❌ BAD: Queries DB every time
var settings = await context.AppSettings.FirstOrDefaultAsync();
var gstRate = settings.DefaultGSTRate;

// ✅ GOOD: Uses cached settings
var settingsService = new SettingsService();
var gstRate = await settingsService.GetGstRateAsync();  // Cached for 5 minutes
```

---

## 🎯 Testing Quick Start

### Unit Test Example
```csharp
using Xunit;
using AuroraInvoice.Services;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomer_SetsCreatedDateInUtc()
    {
        // Arrange
        var service = new CustomerService(new AuditService());
        var customer = new Customer { Name = "Test" };

        // Act
        var result = await service.CreateCustomerAsync(customer);

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.CreatedDate.Kind);
    }
}
```

---

## 🔗 Useful Links

- **Entity Framework Core**: https://docs.microsoft.com/ef/core/
- **WPF Documentation**: https://docs.microsoft.com/wpf/
- **QuestPDF**: https://www.questpdf.com/
- **xUnit Testing**: https://xunit.net/

---

## 📝 Code Review Checklist

Before committing code, check:

- [ ] No `DateTime.Now` (use `DateTimeProvider.UtcNow`)
- [ ] No magic numbers (use `AppConstants`)
- [ ] Audit logs for business operations
- [ ] Error handling with try-catch
- [ ] Transactions for multi-step operations
- [ ] XML documentation comments
- [ ] Service layer instead of direct DB access
- [ ] Proper null checking
- [ ] Resource cleanup (using statements)

---

**Quick Ref Version**: 1.0
**Last Updated**: 2025-10-22
