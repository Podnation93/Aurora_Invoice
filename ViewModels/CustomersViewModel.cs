using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AuroraInvoice.Models;
using AuroraInvoice.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraInvoice.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ICommand NewCustomerCommand { get; }
    public ICommand<Customer> EditCustomerCommand { get; }
    public ICommand<Customer> DeleteCustomerCommand { get; }

    public CustomersViewModel(ICustomerService customerService, IDialogService dialogService)
    {
        _customerService = customerService;
        _dialogService = dialogService;
        NewCustomerCommand = new AsyncRelayCommand(CreateNewCustomer);
        EditCustomerCommand = new AsyncRelayCommand<Customer>(EditCustomer);
        DeleteCustomerCommand = new AsyncRelayCommand<Customer>(DeleteCustomer);

        LoadCustomers();
    }

    private async Task LoadCustomers()
    {
        var (customers, totalCount) = await _customerService.SearchCustomersAsync(SearchText);
        Customers = new ObservableCollection<Customer>(customers);
    }

    private async Task CreateNewCustomer()
    {        
        var result = _dialogService.ShowCustomerDialog();
        if (result == true)
        {
            await LoadCustomers();
        }
    }

    private async Task EditCustomer(Customer? customer)
    {        
        if (customer == null) return;
        var result = _dialogService.ShowCustomerDialog(customer);
        if (result == true)
        {
            await LoadCustomers();
        }
    }
    private async Task DeleteCustomer(Customer? customer)
    {        
        if (customer == null) return;

        try
        {
            await _customerService.DeleteCustomerAsync(customer.Id);
            await LoadCustomers();
        }
        catch (System.Exception)
        {
            // Handle exceptions (e.g., show a message to the user)
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadCustomers();
    }
}
