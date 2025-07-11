using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MauiStudyApp.ViewModels;

public partial class Page1ViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Customer>? customers;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InitializeCommand))]
    private bool isInitialized;

    [RelayCommand(CanExecute = nameof(CanInitialize))]
    private async Task InitializeAsync()
    {
        Customers = new ObservableCollection<Customer>(await DummyService.GetCustomersAsync());
        IsInitialized = true;
    }

    private bool CanInitialize() => !IsInitialized;
}


public class Customer
{
    public int ID { get; set; }

    public string Name { get; set; }
}

public static class DummyService
{
    public static async Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        await Task.Delay(2000);
        return new List<Customer>() {
        new Customer(){ ID = 1, Name = "Jim" },
        new Customer(){ ID = 2, Name = "Bob" }
        };
    }
}