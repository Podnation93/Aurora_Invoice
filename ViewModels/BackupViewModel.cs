using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System;

namespace AuroraInvoice.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly BackupService _backupService;

    [ObservableProperty]
    private string _databaseLocation;

    [ObservableProperty]
    private string _backupFolderPath;

    [ObservableProperty]
    private string _lastBackupText = "Last backup: Never";

    [ObservableProperty]
    private ObservableCollection<object> _backups = new();

    [ObservableProperty]
    private object? _selectedBackup;

    public ICommand CreateBackupCommand { get; }
    public ICommand RestoreBackupCommand { get; }
    public ICommand RefreshBackupsListCommand { get; }

    public BackupViewModel(DatabaseService dbService, BackupService backupService)
    {
        _dbService = dbService;
        _backupService = backupService;

        DatabaseLocation = _dbService.GetDatabasePath();
        BackupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AuroraInvoice Backups");

        CreateBackupCommand = new AsyncRelayCommand(CreateBackup);
        RestoreBackupCommand = new AsyncRelayCommand(RestoreBackup, CanRestoreBackup);
        RefreshBackupsListCommand = new RelayCommand(LoadBackupsList);

        LoadBackupsList();
    }

    private async Task CreateBackup()
    {
        try
        {
            var backupPath = await _backupService.CreateBackupAsync(BackupFolderPath);
            LastBackupText = $"Last backup: {DateTime.Now:dd/MM/yyyy HH:mm}";
            LoadBackupsList();
        }
        catch (Exception)
        {
            // Handle exceptions
        }
    }

    private async Task RestoreBackup()
    {
        if (SelectedBackup is not null)
        {
            dynamic selectedItem = SelectedBackup;
            string backupPath = selectedItem.Path;
            try
            {
                await _backupService.RestoreBackupAsync(backupPath);
                // Handle restart
            }
            catch (Exception)
            {
                // Handle exceptions
            }
        }
    }

    private bool CanRestoreBackup()
    {
        return SelectedBackup != null;
    }

    private void LoadBackupsList()
    {
        try
        {
            var backups = _backupService.GetAvailableBackups(BackupFolderPath);
            Backups.Clear();
            if (backups.Count == 0)
            {
                Backups.Add("No backups found");
            }
            else
            {
                foreach (var backup in backups)
                {
                    var fileName = Path.GetFileName(backup);
                    Backups.Add(new { Name = fileName, Path = backup });
                }
            }
        }
        catch
        {
            Backups.Clear();
            Backups.Add("Error loading backups");
        }
    }
}
