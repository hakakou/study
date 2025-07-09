using DevExpress.Maui.Core;
using MauiStudyApp.ViewModels;
using Microsoft.Maui.Layouts;

namespace MauiStudyApp.Views;

public partial class DefaultPage : ContentPage
{
    public DefaultPage(DefaultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        string oldText = e.OldTextValue;
        string newText = e.NewTextValue;
        System.Console.WriteLine($"Old Text: {oldText}, New Text: {newText}");
    }

    private void Entry_Completed(object sender, EventArgs e)
    {
        string text = ((Entry)sender).Text;
        System.Console.WriteLine($"Entry Completed with Text: {text}");
    }
}
