using DevExpress.Maui.Core;
using MauiStudyApp.ViewModels;
using Microsoft.Maui.Layouts;

namespace MauiStudyApp.Views;

public partial class Page4 : ContentPage
{
    public Page4(Page4ViewModel page)
    {
        InitializeComponent();
        BindingContext = page;
    }

}
