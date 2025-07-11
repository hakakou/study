using CommunityToolkit.Mvvm.Messaging;
using MauiStudyApp.ViewModels;

namespace MauiStudyApp.Views;

public partial class Page3 : ContentPage, IRecipient<LoggedInUserChangedMessage>
{
    public Page3(Page3ViewModel page)
    {
        InitializeComponent();
        BindingContext = page;


        WeakReferenceMessenger.Default.Register<CustomerAddedMessage>(this, static (r, m) =>
        {
            // Handle the message here, with r being the recipient and m being the
            // input message. Using the recipient passed as input makes it so that
            // the lambda expression doesn't capture "this", improving performance.
            
            ((Page3)r).customersCollectionView.ScrollTo(m.Value,
                position: ScrollToPosition.End, animate: true);
        });

        WeakReferenceMessenger.Default.Register<LoggedInUserChangedMessage>(this);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.UnregisterAll(this); 
    }

    void IRecipient<LoggedInUserChangedMessage>.Receive(LoggedInUserChangedMessage message)
    {
    }
}
