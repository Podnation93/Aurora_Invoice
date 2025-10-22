using System.Windows.Controls;
using AuroraInvoice.ViewModels;

namespace AuroraInvoice.Views
{
    public partial class BackupPage : Page
    {
        public BackupPage(BackupViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
