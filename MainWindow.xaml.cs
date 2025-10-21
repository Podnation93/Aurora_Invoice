using System.Windows;
using System.Windows.Controls;
using AuroraInvoice.Views;

namespace AuroraInvoice;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Navigate to Dashboard by default
        MainFrame.Navigate(new DashboardPage());
    }

    private void NavigationButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton || MainFrame == null)
            return;

        Page? pageToNavigate = radioButton.Name switch
        {
            "DashboardNav" => new DashboardPage(),
            "InvoicesNav" => new InvoicesPage(),
            "CustomersNav" => new CustomersPage(),
            "ExpensesNav" => new ExpensesPage(),
            "ReportsNav" => new ReportsPage(),
            "SettingsNav" => new SettingsPage(),
            "BackupNav" => new BackupPage(),
            _ => null
        };

        if (pageToNavigate != null)
        {
            MainFrame.Navigate(pageToNavigate);
        }
    }
}