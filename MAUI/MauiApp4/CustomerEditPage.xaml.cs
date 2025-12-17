using MauiApp4.ViewModels;

namespace MauiApp4;

public partial class CustomerEditPage : ContentPage
{
    public CustomerEditPage(CustomerEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
