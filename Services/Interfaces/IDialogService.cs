using System.Threading.Tasks;
using AuroraInvoice.Models;

namespace AuroraInvoice.Services.Interfaces;

public interface IDialogService
{
    bool? ShowCustomerDialog(Customer? customer = null);
    bool? ShowExpenseDialog(Expense? expense = null);
    bool? ShowInvoiceDialog(Invoice? invoice = null);
    void ShowMessageBox(string message, string title, System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon);
}
