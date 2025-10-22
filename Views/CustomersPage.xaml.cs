using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class CustomersPage : Page
    {
        public CustomersPage(CustomersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
