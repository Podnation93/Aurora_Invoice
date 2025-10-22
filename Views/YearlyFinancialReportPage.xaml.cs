using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class YearlyFinancialReportPage : Page
    {
        public YearlyFinancialReportPage(YearlyFinancialReportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
