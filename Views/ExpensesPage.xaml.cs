using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class ExpensesPage : Page
    {
        public ExpensesPage(ExpensesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}

