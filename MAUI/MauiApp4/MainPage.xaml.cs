namespace MauiApp4;

public partial class MainPage : ContentPage
{  
    public MainPage(ViewModels.MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
