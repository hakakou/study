using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MauiApp4.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<Customer> Customers { get; set; }

    [ObservableProperty]
    public partial bool Refreshing { get; set; }

    [RelayCommand]
    void Showing()
    {
        Refreshing = true;
    }

    [RelayCommand]
    void DeleteCustomer(Customer customer)
    {
        CrmContext context = new CrmContext();
        context.Customers.Remove(customer);
        context.SaveChanges();
        // Update the ObservableCollection also
        Customers.Remove(customer);
    }

    [RelayCommand]
    async Task ShowNewFormAsync()
    {
        await Shell.Current.GoToAsync(nameof(CustomerEditPage),
            parameters: new Dictionary<string, object>
            {
                { "ParentRefreshAction", (Func<Customer, Task>)RefreshAddedAsync },
                { "Item", new Customer() },
            });
    }

    Task RefreshAddedAsync(Customer customer)
    {
        var found = Customers.FirstOrDefault(c => c.Id == customer.Id);
        if (found != null)
        {
            Customers.Add(customer);
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    async Task ShowDetailAsync(Customer c)
    {
        await Shell.Current.GoToAsync(nameof(CustomerEditPage),
            parameters: new Dictionary<string, object>
            {
                { "ParentRefreshAction", (Func<Customer, Task>)RefreshAddedAsync },
                { "Item", c },
            });
    }

    [RelayCommand]
    async Task ShowDragDropDemoAsync()
    {
        await Shell.Current.GoToAsync(nameof(DragDropDemoPage));
    }

    [RelayCommand]
    async Task LoadCustomersAsync()
    {
        await Task.Run(() =>
        {
            using CrmContext context = new CrmContext();
            Customers = new ObservableCollection<Customer>(context.Customers);
        });
        Refreshing = false;
    }
}
