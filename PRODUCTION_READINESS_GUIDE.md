# Aurora Invoice - Production Readiness Guide

## Overview

This document outlines the changes implemented to make Aurora Invoice production-ready, addressing all critical issues identified in the code review. The implementation has been partially completed with **core infrastructure** in place. This guide provides instructions for completing the remaining work.

---

## Implementation Status

### ✅ **COMPLETED** - Core Infrastructure (Phase 1)

The following critical foundation components have been implemented:

1. **Common Infrastructure** (`Common/` folder)
   - `AppConstants.cs` - Centralized constants replacing magic numbers
   - `DateTimeProvider.cs` - UTC-based DateTime handling to prevent DST issues
   - `AppConfiguration.cs` - Configuration system for future appsettings.json support

2. **Service Interfaces** (`Services/Interfaces/` folder)
   - `ISettingsService.cs` - Settings management interface
   - `IAuditService.cs` - Audit logging interface
   - `ICustomerService.cs` - Customer operations interface

3. **Core Services** (`Services/` folder)
   - `SettingsService.cs` - Settings caching with thread-safety
   - `AuditService.cs` - Business operation audit trail
   - `CustomerService.cs` - Customer management with pagination
   - `BackupService.cs` - **CRITICAL FIX** - Added validation and rollback

4. **New Models** (`Models/` folder)
   - `AuditLog.cs` - Audit log entity for tracking operations

5. **Database Improvements** (`Data/` folder)
   - Updated `AuroraDbContext.cs` with:
     - Constructor overload for options (enables testing)
     - `AuditLogs` DbSet
     - Improved SQLite connection string for concurrency

---

## 🔧 **REMAINING WORK** - Critical Fixes (Phase 2)

### Priority 1: Exception Handler Race Condition Fix

**File**: `App.xaml.cs`

**Issue**: Async exception handlers can cause database locks when multiple errors occur simultaneously.

**Solution**:

```csharp
using System.Collections.Concurrent;
using AuroraInvoice.Common;

public partial class App : Application
{
    private static readonly ConcurrentQueue<ErrorLog> _errorQueue = new();
    private static CancellationTokenSource? _errorProcessingCts;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SetupExceptionHandlers();

        // Start error queue processor
        _errorProcessingCts = new CancellationTokenSource();
        _ = Task.Run(() => ProcessErrorQueueAsync(_errorProcessingCts.Token));

        QuestPDF.Settings.License = LicenseType.Community;
        await InitializeDatabaseAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _errorProcessingCts?.Cancel();
        base.OnExit(e);
    }

    private void SetupExceptionHandlers()
    {
        DispatcherUnhandledException += (sender, e) =>
        {
            _errorQueue.Enqueue(new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Critical",
                Source = "App.DispatcherUnhandledException",
                Message = e.Exception.Message,
                StackTrace = e.Exception.StackTrace,
                AdditionalInfo = e.Exception.InnerException?.Message,
                IsResolved = false
            });

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                _errorQueue.Enqueue(new ErrorLog
                {
                    Timestamp = DateTimeProvider.UtcNow,
                    Severity = "Critical",
                    Source = "AppDomain.UnhandledException",
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    IsResolved = false
                });
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            _errorQueue.Enqueue(new ErrorLog
            {
                Timestamp = DateTimeProvider.UtcNow,
                Severity = "Error",
                Source = "TaskScheduler.UnobservedTaskException",
                Message = e.Exception.Message,
                StackTrace = e.Exception.StackTrace,
                IsResolved = false
            });

            e.SetObserved();
        };
    }

    private async Task ProcessErrorQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_errorQueue.TryDequeue(out var errorLog))
            {
                try
                {
                    using var context = new AuroraDbContext();
                    context.ErrorLogs.Add(errorLog);
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // If logging fails, re-queue and wait longer
                    _errorQueue.Enqueue(errorLog);
                    await Task.Delay(1000, cancellationToken);
                }
            }

            await Task.Delay(AppConstants.ErrorQueueProcessingDelayMs, cancellationToken);
        }
    }
}
```

---

### Priority 2: Update All Model DateTime Properties

**Files**: All models in `Models/` folder

**Issue**: Using `DateTime.Now` causes DST bugs and timezone issues.

**Solution**: Replace all `DateTime.Now` with `DateTimeProvider.UtcNow`

**Example for `Customer.cs`**:

```csharp
using AuroraInvoice.Common;

public class Customer
{
    // ... other properties ...

    public DateTime CreatedDate { get; set; } = DateTimeProvider.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    // ... navigation properties ...
}
```

**Apply to these files**:
- `Models/Customer.cs` - Line 32
- `Models/Invoice.cs` - Lines 24, 26, 43
- `Models/Expense.cs` - Lines 14, 48
- `Models/ExpenseCategory.cs` - Line (check CreatedDate)
- `Models/AppSettings.cs` - Line (check CreatedDate)

---

### Priority 3: Create Database Migration for AuditLog

**Command**:
```bash
dotnet ef migrations add AddAuditLogTable
```

This will create a migration for the new `AuditLog` table.

---

### Priority 4: Update GstCalculationService

**File**: `Services/GstCalculationService.cs`

**Issue**: Hard-coded GST rate of 0.10m

**Solution**:

```csharp
using AuroraInvoice.Services.Interfaces;

public class GstCalculationService
{
    private readonly ISettingsService _settingsService;

    public GstCalculationService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<decimal> CalculateGstFromTotalAsync(decimal totalAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        if (gstRate <= 0) return 0;

        return Math.Round(totalAmount * (gstRate / (1 + gstRate)), 2);
    }

    public async Task<decimal> CalculateGstToAddAsync(decimal baseAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        if (gstRate <= 0) return 0;

        return Math.Round(baseAmount * gstRate, 2);
    }

    public async Task<decimal> CalculateTotalWithGstAsync(decimal baseAmount)
    {
        var gstAmount = await CalculateGstToAddAsync(baseAmount);
        return baseAmount + gstAmount;
    }

    public async Task<decimal> CalculateBaseFromTotalAsync(decimal totalAmount)
    {
        var gstRate = await _settingsService.GetGstRateAsync();
        if (gstRate <= 0) return totalAmount;

        return Math.Round(totalAmount / (1 + gstRate), 2);
    }

    // Synchronous versions for backward compatibility (use cached settings)
    public decimal CalculateGstFromTotal(decimal totalAmount, decimal gstRate)
    {
        if (gstRate <= 0) return 0;
        return Math.Round(totalAmount * (gstRate / (1 + gstRate)), 2);
    }

    public decimal CalculateGstToAdd(decimal baseAmount, decimal gstRate)
    {
        if (gstRate <= 0) return 0;
        return Math.Round(baseAmount * gstRate, 2);
    }

    public decimal CalculateTotalWithGst(decimal baseAmount, decimal gstRate)
    {
        var gstAmount = CalculateGstToAdd(baseAmount, gstRate);
        return baseAmount + gstAmount;
    }

    public decimal CalculateBaseFromTotal(decimal totalAmount, decimal gstRate)
    {
        if (gstRate <= 0) return totalAmount;
        return Math.Round(totalAmount / (1 + gstRate), 2);
    }
}
```

---

### Priority 5: Fix InvoiceDialog Validation and Transactions

**File**: `Views/InvoiceDialog.xaml.cs`

**Critical Changes Needed**:

1. **Add validation method**:

```csharp
private async Task<(bool IsValid, string Error)> ValidateInvoiceAsync()
{
    if (CustomerComboBox.SelectedItem == null)
        return (false, "Please select a customer.");

    if (string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text))
        return (false, "Please enter an invoice number.");

    if (_items.Count == 0)
        return (false, "Please add at least one item to the invoice.");

    if (!InvoiceDatePicker.SelectedDate.HasValue)
        return (false, "Please select an invoice date.");

    if (!DueDatePicker.SelectedDate.HasValue)
        return (false, "Please select a due date.");

    if (DueDatePicker.SelectedDate < InvoiceDatePicker.SelectedDate)
        return (false, "Due date cannot be before invoice date.");

    if (_items.Any(i => i.Quantity <= 0))
        return (false, "All item quantities must be greater than zero.");

    if (_items.Any(i => i.UnitPrice < 0))
        return (false, "Unit prices cannot be negative.");

    // Check for duplicate invoice number (only when creating new)
    if (!_isEditMode)
    {
        using var context = new AuroraDbContext();
        var exists = await context.Invoices
            .AnyAsync(i => i.InvoiceNumber == InvoiceNumberTextBox.Text.Trim());
        if (exists)
            return (false, $"Invoice number '{InvoiceNumberTextBox.Text.Trim()}' already exists.");
    }

    return (true, string.Empty);
}
```

2. **Add transaction to Save_Click**:

```csharp
private async void Save_Click(object sender, RoutedEventArgs e)
{
    // Validate first
    var (isValid, error) = await ValidateInvoiceAsync();
    if (!isValid)
    {
        MessageBox.Show(error, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    try
    {
        using var context = new AuroraDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var selectedCustomer = (Customer)CustomerComboBox.SelectedItem;
            var subtotal = _items.Sum(i => i.LineTotal);
            var gstTotal = _items.Sum(i => i.GSTAmount);
            var total = subtotal + gstTotal;

            if (_isEditMode && _invoice != null)
            {
                var invoiceToUpdate = await context.Invoices
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == _invoice.Id);

                if (invoiceToUpdate != null)
                {
                    invoiceToUpdate.CustomerId = selectedCustomer.Id;
                    invoiceToUpdate.InvoiceNumber = InvoiceNumberTextBox.Text.Trim();
                    invoiceToUpdate.InvoiceDate = InvoiceDatePicker.SelectedDate.Value;
                    invoiceToUpdate.DueDate = DueDatePicker.SelectedDate.Value;
                    invoiceToUpdate.Status = (InvoiceStatus)StatusComboBox.SelectedIndex;
                    invoiceToUpdate.SubTotal = subtotal;
                    invoiceToUpdate.GSTAmount = gstTotal;
                    invoiceToUpdate.TotalAmount = total;
                    invoiceToUpdate.Notes = NotesTextBox.Text.Trim();
                    invoiceToUpdate.ModifiedDate = DateTimeProvider.UtcNow;

                    context.InvoiceItems.RemoveRange(invoiceToUpdate.InvoiceItems);

                    foreach (var item in _items)
                    {
                        invoiceToUpdate.InvoiceItems.Add(new InvoiceItem
                        {
                            Description = item.Description,
                            ServiceDate = item.ServiceDate,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            GSTRate = item.GSTRate,
                            LineTotal = item.LineTotal,
                            GSTAmount = item.GSTAmount
                        });
                    }
                }
            }
            else
            {
                var newInvoice = new Invoice
                {
                    CustomerId = selectedCustomer.Id,
                    InvoiceNumber = InvoiceNumberTextBox.Text.Trim(),
                    InvoiceDate = InvoiceDatePicker.SelectedDate.Value,
                    DueDate = DueDatePicker.SelectedDate.Value,
                    Status = (InvoiceStatus)StatusComboBox.SelectedIndex,
                    SubTotal = subtotal,
                    GSTAmount = gstTotal,
                    TotalAmount = total,
                    Notes = NotesTextBox.Text.Trim(),
                    CreatedDate = DateTimeProvider.UtcNow
                };

                foreach (var item in _items)
                {
                    newInvoice.InvoiceItems.Add(item);
                }

                context.Invoices.Add(newInvoice);

                // Update next invoice number
                var settings = await context.AppSettings.FirstOrDefaultAsync();
                if (settings != null)
                {
                    settings.NextInvoiceNumber++;
                    settings.ModifiedDate = DateTimeProvider.UtcNow;
                }
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error saving invoice: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

---

### Priority 6: Fix InvoicePdfService Template Respect

**File**: `Services/InvoicePdfService.cs`

This is a large fix. Key changes needed:

1. **Wrap all template-dependent sections with conditionals**
2. **Example for customer info section** (around line 169-192):

```csharp
if (template.ShowCustomerName)
{
    leftColumn.Item().PaddingTop(8).Text(invoice.Customer?.Name ?? "")
        .FontSize(12)
        .Bold();
}

if (template.ShowCustomerContactPerson && !string.IsNullOrWhiteSpace(invoice.Customer?.ContactPerson))
{
    leftColumn.Item().Text(invoice.Customer.ContactPerson)
        .FontSize(10)
        .FontColor(Colors.Grey.Darken1);
}

if (template.ShowCustomerAddress && !string.IsNullOrWhiteSpace(invoice.Customer?.Address))
{
    leftColumn.Item().Text(invoice.Customer.Address)
        .FontSize(10)
        .FontColor(Colors.Grey.Darken1);
}

if (template.ShowCustomerEmail && !string.IsNullOrWhiteSpace(invoice.Customer?.Email))
{
    leftColumn.Item().Text(invoice.Customer.Email)
        .FontSize(10)
        .FontColor(Colors.Grey.Darken1);
}
```

3. **Apply similar conditionals to**:
   - Business info section (header)
   - Date fields (invoice date, due date)
   - Line item columns
   - Totals section (subtotal, GST, grand total)
   - Footer sections

---

### Priority 7: Refactor CustomersPage

**File**: `Views/CustomersPage.xaml.cs`

**Replace with service-based approach**:

```csharp
using AuroraInvoice.Services;
using AuroraInvoice.Services.Interfaces;

public partial class CustomersPage : Page
{
    private readonly ICustomerService _customerService;
    private int _currentPage = 1;
    private int _pageSize = AppConstants.DefaultPageSize;
    private int _totalCount = 0;

    public CustomersPage()
    {
        InitializeComponent();

        // Initialize service
        var auditService = new AuditService();
        _customerService = new CustomerService(auditService);

        Loaded += CustomersPage_Loaded;
    }

    private async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var (customers, totalCount) = await _customerService.GetCustomersAsync(_currentPage, _pageSize);
            _totalCount = totalCount;

            CustomersGrid.ItemsSource = customers;

            // Show/hide empty state
            EmptyState.Visibility = customers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CustomersGrid.Visibility = customers.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            // Update pagination info (if you add pagination UI)
            // PageInfoText.Text = $"Page {_currentPage} of {Math.Ceiling((double)_totalCount / _pageSize)}";
        }
        catch (Exception ex)
        {
            await LoggingService.LogErrorAsync(ex, "CustomersPage.LoadCustomersAsync");
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text;

        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                await LoadCustomersAsync();
            }
            else
            {
                var (customers, totalCount) = await _customerService.SearchCustomersAsync(searchText, 1, _pageSize);
                CustomersGrid.ItemsSource = customers;
            }
        }
        catch (Exception ex)
        {
            await LoggingService.LogErrorAsync(ex, "CustomersPage.SearchBox_TextChanged");
        }
    }

    private async void DeleteCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Customer customer)
        {
            // Check invoice count first
            var invoiceCount = await _customerService.GetInvoiceCountAsync(customer.Id);

            if (invoiceCount > 0)
            {
                MessageBox.Show(
                    $"Cannot delete '{customer.Name}' because {invoiceCount} invoice(s) are associated with this customer.\n\n" +
                    "Please delete or reassign the invoices first.",
                    "Cannot Delete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{customer.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(customer.Id);

                    MessageBox.Show("Customer deleted successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    await LoggingService.LogErrorAsync(ex, "CustomersPage.DeleteCustomer_Click");
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // ... rest of the event handlers ...
}
```

---

## 📋 **ADDITIONAL RECOMMENDATIONS** - Phase 3

### Create .editorconfig for Code Style

**File**: `.editorconfig` (root of solution)

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
trim_trailing_whitespace = true
insert_final_newline = true
indent_style = space
indent_size = 4

[*.cs]
# Naming conventions
dotnet_naming_rule.private_fields_should_be_camel_case.severity = warning
dotnet_naming_rule.private_fields_should_be_camel_case.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camel_case.style = camel_case_underscore_style

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.camel_case_underscore_style.capitalization = camel_case
dotnet_naming_style.camel_case_underscore_style.required_prefix = _

# Code style
csharp_prefer_braces = true:warning
csharp_prefer_simple_using_statement = true:suggestion
dotnet_sort_system_directives_first = true

[*.{csproj,props,targets}]
indent_size = 2

[*.{json,yml,yaml}]
indent_size = 2
```

### Create appsettings.json

**File**: `appsettings.json`

```json
{
  "Database": {
    "Provider": "SQLite",
    "FilePath": null,
    "BackupPath": null,
    "ConnectionString": "Cache=Shared;Mode=ReadWriteCreate"
  },
  "Logging": {
    "RetentionDays": 30,
    "MaxEntries": 10000,
    "EnableDebugLogging": false
  },
  "UI": {
    "DashboardRecentInvoices": 10,
    "PageSize": 50,
    "EnableAnimations": true
  }
}
```

**Add to .csproj**:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
</ItemGroup>
```

---

## 🧪 **TESTING** - Phase 4

### Create Test Project

```bash
dotnet new xunit -n AuroraInvoice.Tests
cd AuroraInvoice.Tests
dotnet add reference ../AuroraInvoice.csproj
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
```

### Sample Test Class

**File**: `AuroraInvoice.Tests/Services/CustomerServiceTests.cs`

```csharp
using Xunit;
using AuroraInvoice.Services;
using AuroraInvoice.Models;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomerAsync_ShouldSetCreatedDate()
    {
        // Arrange
        var auditService = new AuditService();
        var customerService = new CustomerService(auditService);
        var customer = new Customer { Name = "Test Customer" };

        // Act
        var result = await customerService.CreateCustomerAsync(customer);

        // Assert
        Assert.NotEqual(default(DateTime), result.CreatedDate);
        Assert.True(result.CreatedDate.Kind == DateTimeKind.Utc);
    }
}
```

---

## 📦 **DEPLOYMENT CHECKLIST**

### Before Production Release:

- [ ] Run all database migrations
- [ ] Update all `DateTime.Now` to `DateTimeProvider.UtcNow`
- [ ] Fix exception handler race conditions
- [ ] Add transaction support to InvoiceDialog
- [ ] Fix InvoicePdfService template respect
- [ ] Refactor CustomersPage with service layer
- [ ] Create and run unit tests (minimum 60% coverage)
- [ ] Performance test with 10,000+ customers
- [ ] Test backup and restore functionality
- [ ] Test concurrent operations (multiple windows)
- [ ] Review and update XML documentation
- [ ] Create user manual
- [ ] Setup automated build pipeline

### Performance Targets:

- Dashboard load: < 2 seconds
- Customer search: < 1 second for 10,000 records
- PDF generation: < 3 seconds
- Backup creation: < 10 seconds for 1GB database
- Database startup: < 5 seconds

---

## 🔥 **CRITICAL ISSUES FIXED**

1. ✅ **Backup Restore Safety** - Added validation and automatic rollback
2. ✅ **Settings Caching** - Implemented thread-safe caching with `SemaphoreSlim`
3. ✅ **Audit Logging** - Full business operation tracking
4. ✅ **Database Concurrency** - Improved SQLite connection string
5. ✅ **Service Layer** - Proper separation of concerns with interfaces
6. ✅ **DateTimeProvider** - UTC-based time handling
7. ✅ **Constants** - Eliminated magic numbers and strings

---

## 📊 **ESTIMATED COMPLETION TIME**

- **Phase 1** (Completed): 40 hours
- **Phase 2** (Critical Fixes): 60 hours
- **Phase 3** (Additional Improvements): 80 hours
- **Phase 4** (Testing): 60 hours

**Total Remaining**: ~200 hours

---

## 📚 **RESOURCES**

- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [WPF MVVM Pattern](https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [QuestPDF Documentation](https://www.questpdf.com/)
- [xUnit Testing](https://xunit.net/)

---

## 🆘 **SUPPORT**

For questions or issues during implementation:

1. Review this guide thoroughly
2. Check existing code for patterns
3. Refer to interface definitions for contracts
4. Review audit logs for operation tracking

---

**Document Version**: 1.0
**Last Updated**: 2025-10-22
**Author**: AI Code Review System
