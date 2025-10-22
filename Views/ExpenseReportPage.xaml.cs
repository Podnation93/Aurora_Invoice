using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class ExpenseReportPage : Page
    {
        public ExpenseReportPage(ExpenseReportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
