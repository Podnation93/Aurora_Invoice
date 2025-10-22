using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class GstSummaryPage : Page
    {
        public GstSummaryPage(GstSummaryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
