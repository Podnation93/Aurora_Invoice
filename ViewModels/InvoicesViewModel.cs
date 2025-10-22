using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraInvoice.ViewModels;

public partial class InvoicesViewModel : ObservableObject
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    private InvoiceSummary _summary = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private InvoiceStatus? _selectedStatus = null;

    public ICommand NewInvoiceCommand { get; }
    public ICommand<Invoice> EditInvoiceCommand { get; }
    public ICommand<Invoice> DeleteInvoiceCommand { get; }
    public ICommand<Invoice> PreviewInvoiceCommand { get; }
    public ICommand<Invoice> DownloadInvoiceCommand { get; }

    public InvoicesViewModel(IInvoiceService invoiceService, ICustomerService customerService, IDialogService dialogService)
    {
        _invoiceService = invoiceService;
        _customerService = customerService;
        _dialogService = dialogService;

        NewInvoiceCommand = new AsyncRelayCommand(CreateNewInvoice);
        EditInvoiceCommand = new AsyncRelayCommand<Invoice>(EditInvoice);
        DeleteInvoiceCommand = new AsyncRelayCommand<Invoice>(DeleteInvoice);
        PreviewInvoiceCommand = new AsyncRelayCommand<Invoice>(PreviewInvoice);
        DownloadInvoiceCommand = new AsyncRelayCommand<Invoice>(DownloadInvoice);

        LoadInvoices();
    }

    private async Task LoadInvoices()
    {
        await _invoiceService.UpdateOverdueInvoicesAsync();
        var invoices = await _invoiceService.SearchInvoicesAsync(SearchText, SelectedStatus);
        Invoices = new ObservableCollection<Invoice>(invoices);
        Summary = await _invoiceService.GetInvoiceSummaryAsync();
    }

    private async Task CreateNewInvoice()
    {
        var result = _dialogService.ShowInvoiceDialog();
        if (result == true)
        {
            await LoadInvoices();
        }
    }

    private async Task EditInvoice(Invoice? invoice)
    {
        if (invoice == null) return;
        var result = _dialogService.ShowInvoiceDialog(invoice);
        if (result == true)
        {
            await LoadInvoices();
        }
    }

    private async Task DeleteInvoice(Invoice? invoice)
    {
        if (invoice == null) return;
        await _invoiceService.DeleteInvoiceAsync(invoice.Id);
        await LoadInvoices();
    }

    private async Task PreviewInvoice(Invoice? invoice)
    {
        if (invoice == null) return;
        // Requires a PDF service/helper
    }

    private async Task DownloadInvoice(Invoice? invoice)
    {
        if (invoice == null) return;
        // Requires a PDF service/helper and save file dialog service
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadInvoices();
    }

    partial void OnSelectedStatusChanged(InvoiceStatus? value)
    {
        _ = LoadInvoices();
    }
}
