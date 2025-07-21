using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;

namespace MauiStudyApp2;

public partial class Page2 : ContentPage
{
    public static CancellationToken DefaultCancellationToken => CancellationToken.None;

    public Page2()
    {
        InitializeComponent();
        BindingContext = new MyViewModel();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Application.Current.UserAppTheme = 
            Application.Current.UserAppTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
    }
}

public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    string statusMessage = "Let's run!";

    [RelayCommand]
    async Task HamsterRunAsync()
    {
        StatusMessage = "Running";
        await Task.Delay(3000);
        StatusMessage = "Complete! Let's run again?";
    }
}