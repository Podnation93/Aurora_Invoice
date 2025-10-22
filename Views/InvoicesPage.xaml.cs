using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class InvoicesPage : Page
    {
        public InvoicesPage(InvoicesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
