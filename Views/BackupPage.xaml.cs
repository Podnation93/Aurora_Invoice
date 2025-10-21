using System.IO;
using System.Windows;
using System.Windows.Controls;
using AuroraInvoice.Data;
using AuroraInvoice.Services;

namespace AuroraInvoice.Views;

public partial class BackupPage : Page
{
    private readonly DatabaseService _dbService;
    private readonly BackupService _backupService;
    private string _backupFolderPath;

    public BackupPage()
    {
        InitializeComponent();
        using var context = new AuroraDbContext();
        _dbService = new DatabaseService(context);
        _backupService = new BackupService(_dbService);

        _backupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AuroraInvoice Backups");

        Loaded += BackupPage_Loaded;
    }

    private void BackupPage_Loaded(object sender, RoutedEventArgs e)
    {
        DatabaseLocationText.Text = _dbService.GetDatabasePath();
        BackupFolderTextBox.Text = _backupFolderPath;
        LoadBackupsList();
    }

    private void BrowseBackupFolder_Click(object sender, RoutedEventArgs e)
    {
        // For now, use default Documents folder
        // In production, could use Windows.Storage.Pickers or third-party library
        MessageBox.Show("Using default backup folder in Documents.\n\nYou can manually enter a path in the textbox if needed.", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void CreateBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var button = sender as Button;
            if (button != null) button.IsEnabled = false;

            var backupPath = await _backupService.CreateBackupAsync(_backupFolderPath);

            LastBackupText.Text = $"Last backup: {DateTime.Now:dd/MM/yyyy HH:mm}";

            MessageBox.Show($"Backup created successfully!\n\nLocation: {backupPath}", "Backup Complete",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadBackupsList();

            if (button != null) button.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating backup: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadBackupsList()
    {
        try
        {
            var backups = _backupService.GetAvailableBackups(_backupFolderPath);
            BackupsListBox.Items.Clear();

            foreach (var backup in backups)
            {
                var fileName = Path.GetFileName(backup);
                BackupsListBox.Items.Add(new { Name = fileName, Path = backup });
            }

            if (backups.Count == 0)
            {
                BackupsListBox.Items.Add("No backups found");
            }
        }
        catch
        {
            BackupsListBox.Items.Clear();
            BackupsListBox.Items.Add("Error loading backups");
        }
    }

    private void BackupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RestoreButton.IsEnabled = BackupsListBox.SelectedItem != null && BackupsListBox.SelectedItem is not string;
    }

    private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        if (BackupsListBox.SelectedItem == null) return;

        dynamic selectedItem = BackupsListBox.SelectedItem;
        string backupPath = selectedItem.Path;

        var result = MessageBox.Show(
            "Are you sure you want to restore from this backup?\n\nThis will replace all current data. The current data will be backed up first.\n\nThis action cannot be undone.",
            "Confirm Restore",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _backupService.RestoreBackupAsync(backupPath);

                MessageBox.Show("Database restored successfully!\n\nThe application will now restart.", "Restore Complete",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring backup: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RefreshBackupsList_Click(object sender, RoutedEventArgs e)
    {
        LoadBackupsList();
    }
}
