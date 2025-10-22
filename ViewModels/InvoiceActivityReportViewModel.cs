using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraInvoice.ViewModels
{
    public partial class InvoiceActivityReportViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private DateTime _startDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private InvoiceActivityReport? _report;

        public ICommand GenerateReportCommand { get; }

        public InvoiceActivityReportViewModel(IReportService reportService)
        {
            _reportService = reportService;
            GenerateReportCommand = new AsyncRelayCommand(GenerateReport);
        }

        private async Task GenerateReport()
        {
            Report = await _reportService.GetInvoiceActivityReportAsync(StartDate, EndDate);
        }
    }
}
