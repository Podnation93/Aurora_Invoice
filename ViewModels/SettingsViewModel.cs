using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraInvoice.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private AppSettings _settings = new();

    public ICommand SaveSettingsCommand { get; }

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettings);
        LoadSettings();
    }

    private async Task LoadSettings()
    {
        Settings = await _settingsService.GetSettingsAsync();
    }

    private async Task SaveSettings()
    {        
        await _settingsService.UpdateSettingsAsync(Settings);
    }
}
