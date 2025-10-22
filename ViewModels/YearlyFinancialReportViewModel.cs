using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace AuroraInvoice.ViewModels
{
    public partial class YearlyFinancialReportViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private int _selectedYear = DateTime.Now.Year;

        [ObservableProperty]
        private YearlyFinancialSummary? _summary;

        public ObservableCollection<int> Years { get; } = new(Enumerable.Range(DateTime.Now.Year - 10, 11).Reverse());

        public ICommand GenerateReportCommand { get; }

        public YearlyFinancialReportViewModel(IReportService reportService)
        {
            _reportService = reportService;
            GenerateReportCommand = new AsyncRelayCommand(GenerateReport);
        }

        private async Task GenerateReport()
        {
            Summary = await _reportService.GetYearlyFinancialSummaryAsync(SelectedYear);
        }
    }
}
