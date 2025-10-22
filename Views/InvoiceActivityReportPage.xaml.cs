using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class InvoiceActivityReportPage : Page
    {
        public InvoiceActivityReportPage(InvoiceActivityReportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
