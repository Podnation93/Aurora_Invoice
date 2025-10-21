using System.IO;
using Microsoft.EntityFrameworkCore;
using AuroraInvoice.Models;

namespace AuroraInvoice.Data;

/// <summary>
/// Entity Framework database context for Aurora Invoice
/// </summary>
public class AuroraDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AuroraInvoice",
                "aurora_invoice.db");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Invoice)
            .WithMany(i => i.InvoiceItems)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed default expense categories
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { Id = 1, Name = "Office Supplies", Description = "Stationery, printer supplies, etc.", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 2, Name = "Rent", Description = "Office or workspace rental", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 3, Name = "Utilities", Description = "Electricity, water, internet, phone", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 4, Name = "Travel", Description = "Transportation, accommodation, meals", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 5, Name = "Software Subscriptions", Description = "Cloud services, software licenses", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 6, Name = "Professional Services", Description = "Legal, accounting, consulting", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 7, Name = "Marketing & Advertising", Description = "Promotional materials, online ads", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 8, Name = "Equipment", Description = "Computers, furniture, machinery", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 9, Name = "Insurance", Description = "Business insurance premiums", CreatedDate = seedDate, IsActive = true },
            new ExpenseCategory { Id = 10, Name = "Other", Description = "Miscellaneous expenses", CreatedDate = seedDate, IsActive = true }
        );

        // Seed default app settings
        modelBuilder.Entity<AppSettings>().HasData(
            new AppSettings
            {
                Id = 1,
                BusinessName = "My Business",
                InvoicePrefix = "INV-",
                NextInvoiceNumber = 1,
                DefaultPaymentTermsDays = 30,
                DefaultGSTRate = 0.10m,
                ThemeColor = "#2563eb",
                AccentColor = "#7c3aed",
                CreatedDate = seedDate
            }
        );
    }
}
