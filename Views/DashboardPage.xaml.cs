using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
