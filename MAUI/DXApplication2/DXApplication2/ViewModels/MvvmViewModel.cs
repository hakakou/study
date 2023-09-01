using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DXApplication2.Domain.Data;
using DXApplication2.Domain.Services;
using System.Collections.ObjectModel;

namespace DXApplication2.ViewModels
{
    public partial class MvvmViewModel : ObservableObject
    {

        [ObservableProperty]
        ObservableCollection<Customer>? customers;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitializeCommand))]
        bool isInitialized;

        readonly IDataService dataService;
        public MvvmViewModel(IDataService dataService)
        {
            this.dataService = dataService;
        }

        [RelayCommand(CanExecute = nameof(CanInitialize))]
        async Task InitializeAsync()
        {
            Customers = new ObservableCollection<Customer>(await dataService.GetCustomersAsync());
            IsInitialized = true;
        }

        bool CanInitialize() => !IsInitialized;
    }
}
