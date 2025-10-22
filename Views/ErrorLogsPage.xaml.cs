using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class ErrorLogsPage : Page
    {
        public ErrorLogsPage(ErrorLogsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
