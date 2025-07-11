using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System.Collections.ObjectModel;

namespace MauiStudyApp.ViewModels
{
    public partial class Page3ViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCustomerCommand))]
        ObservableCollection<Customer>? customers = new ObservableCollection<Customer>();

        Bogus.DataSets.Name lorem = new();

        [RelayCommand(CanExecute = nameof(CanAddCustomer))]
        void AddCustomer()
        {
            if (Customers != null)
            {
                var c = new Customer()
                {
                    ID = Customers.Count,
                    Name = lorem.FullName()
                };
                Customers.Add(c);

                WeakReferenceMessenger.Default.Send(new CustomerAddedMessage(c));
                WeakReferenceMessenger.Default.Send(new LoggedInUserChangedMessage(c));
            }
        }
        bool CanAddCustomer()
        {
            return Customers != null;
        }
    }

    public sealed class CustomerAddedMessage : ValueChangedMessage<Customer>
    {
        public CustomerAddedMessage(Customer value) : base(value) { }
    }

    public sealed class LoggedInUserChangedMessage : ValueChangedMessage<Customer>
    {
        public LoggedInUserChangedMessage(Customer value) : base(value) { }
    }
}

