using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraInvoice.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;

    [ObservableProperty]
    private DashboardMetrics _metrics = new();

    [ObservableProperty]
    private ObservableCollection<Invoice> _recentInvoices = new();

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        Metrics = await _dashboardService.GetMonthlyMetricsAsync();
        var recentInvoices = await _dashboardService.GetRecentInvoicesAsync();
        RecentInvoices = new ObservableCollection<Invoice>(recentInvoices);
    }
}
