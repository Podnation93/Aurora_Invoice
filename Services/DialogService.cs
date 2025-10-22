using AuroraInvoice.Data;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using AuroraInvoice.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace AuroraInvoice.Services
{
    public class DialogService : IDialogService
    {
        private readonly IDbContextFactory<AuroraDbContext> _contextFactory;
        private readonly ISettingsService _settingsService;
        private readonly IInvoiceService _invoiceService;

        public DialogService(IDbContextFactory<AuroraDbContext> contextFactory, ISettingsService settingsService, IInvoiceService invoiceService)
        {
            _contextFactory = contextFactory;
            _settingsService = settingsService;
            _invoiceService = invoiceService;
        }

        public bool? ShowCustomerDialog(Customer? customer = null)
        {
            CustomerDialog dialog;
            if (customer == null)
            {
                dialog = new CustomerDialog(_contextFactory);
            }
            else
            {
                dialog = new CustomerDialog(customer, _contextFactory);
            }
            return dialog.ShowDialog();
        }

        public bool? ShowExpenseDialog(Expense? expense = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var categories = context.ExpenseCategories.ToList();
            ExpenseDialog dialog;
            if (expense == null)
            {
                dialog = new ExpenseDialog(categories, _contextFactory, _settingsService);
            }
            else
            {
                dialog = new ExpenseDialog(expense, categories, _contextFactory, _settingsService);
            }
            return dialog.ShowDialog();
        }

        public bool? ShowInvoiceDialog(Invoice? invoice = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var customers = context.Customers.ToList();
            InvoiceDialog dialog;
            if (invoice == null)
            {
                var nextInvoiceNumber = Task.Run(() => _invoiceService.GetNextInvoiceNumberAsync()).GetAwaiter().GetResult();
                dialog = new InvoiceDialog(customers, nextInvoiceNumber, _contextFactory, _settingsService);
            }
            else
            {
                dialog = new InvoiceDialog(invoice, customers, _contextFactory, _settingsService);
            }
            return dialog.ShowDialog();
        }

        public void ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBox.Show(message, title, button, icon);
        }
    }
}
