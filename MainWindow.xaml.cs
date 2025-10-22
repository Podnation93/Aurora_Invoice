using System.Windows;
using System.Windows.Controls;
using AuroraInvoice.Views;
using Microsoft.Extensions.DependencyInjection;

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
        MainFrame.Navigate(App.ServiceProvider.GetRequiredService<DashboardPage>());
    }

    private void NavigationButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton radioButton || MainFrame == null)
            return;

        Page? pageToNavigate = radioButton.Name switch
        {
            "DashboardNav" => App.ServiceProvider.GetRequiredService<DashboardPage>(),
            "InvoicesNav" => App.ServiceProvider.GetRequiredService<InvoicesPage>(),
            "CustomersNav" => App.ServiceProvider.GetRequiredService<CustomersPage>(),
            "ExpensesNav" => App.ServiceProvider.GetRequiredService<ExpensesPage>(),
            "ReportsNav" => App.ServiceProvider.GetRequiredService<ReportsPage>(),
            "SettingsNav" => App.ServiceProvider.GetRequiredService<SettingsPage>(),
            "BackupNav" => App.ServiceProvider.GetRequiredService<BackupPage>(),
            "ErrorLogsNav" => App.ServiceProvider.GetRequiredService<ErrorLogsPage>(),
            _ => null
        };

        if (pageToNavigate != null)
        {
            MainFrame.Navigate(pageToNavigate);
        }
    }
}