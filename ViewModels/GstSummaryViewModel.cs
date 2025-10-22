using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraInvoice.ViewModels
{
    public partial class GstSummaryViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private DateTime _startDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private GstSummary? _summary;

        public ICommand GenerateReportCommand { get; }

        public GstSummaryViewModel(IReportService reportService)
        {
            _reportService = reportService;
            GenerateReportCommand = new AsyncRelayCommand(GenerateReport);
        }

        private async Task GenerateReport()
        {
            Summary = await _reportService.GetGstSummaryAsync(StartDate, EndDate);
        }
    }
}
