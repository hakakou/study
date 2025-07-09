using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiStudyApp.Domain.Data;
using MauiStudyApp.Domain.Services;
using System.Collections.ObjectModel;

namespace MauiStudyApp.ViewModels
{
    public partial class MvvmViewModel : ObservableObject
    {

        [ObservableProperty]
        ObservableCollection<Domain.Data.Customer>? customers;

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
            Customers = new ObservableCollection<Domain.Data.Customer>(await dataService.GetCustomersAsync());
            IsInitialized = true;
        }

        bool CanInitialize() => !IsInitialized;
    }
}
