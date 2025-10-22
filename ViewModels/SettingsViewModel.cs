using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraInvoice.ViewModels;

public partial class SettingsViewModel : ObservableValidator
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    [Required]
    [MaxLength(200)]
    private string _businessName = string.Empty;

    [ObservableProperty]
    [MaxLength(50)]
    private string? _abn;

    [ObservableProperty]
    [MaxLength(500)]
    private string? _businessAddress;

    [ObservableProperty]
    [MaxLength(50)]
    private string? _phone;

    [ObservableProperty]
    [EmailAddress]
    [MaxLength(200)]
    private string? _email;

    [ObservableProperty]
    [MaxLength(20)]
    private string? _invoicePrefix;

    [ObservableProperty]
    [Range(1, int.MaxValue)]
    private int _nextInvoiceNumber;

    [ObservableProperty]
    [Range(0, int.MaxValue)]
    private int _defaultPaymentTermsDays;

    [ObservableProperty]
    [Range(0, 1)]
    private decimal _defaultGSTRate;

    public ICommand SaveSettingsCommand { get; }

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettings, () => !HasErrors);
        LoadSettings();
    }

    private async Task LoadSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        BusinessName = settings.BusinessName;
        ABN = settings.ABN;
        BusinessAddress = settings.BusinessAddress;
        Phone = settings.Phone;
        Email = settings.Email;
        InvoicePrefix = settings.InvoicePrefix;
        NextInvoiceNumber = settings.NextInvoiceNumber;
        DefaultPaymentTermsDays = settings.DefaultPaymentTermsDays;
        DefaultGSTRate = settings.DefaultGSTRate;

        ValidateAllProperties();
    }

    private async Task SaveSettings()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        var settings = await _settingsService.GetSettingsAsync();
        settings.BusinessName = BusinessName;
        settings.ABN = ABN;
        settings.BusinessAddress = BusinessAddress;
        settings.Phone = Phone;
        settings.Email = Email;
        settings.InvoicePrefix = InvoicePrefix;
        settings.NextInvoiceNumber = NextInvoiceNumber;
        settings.DefaultPaymentTermsDays = DefaultPaymentTermsDays;
        settings.DefaultGSTRate = DefaultGSTRate;

        await _settingsService.UpdateSettingsAsync(settings);
    }
}
