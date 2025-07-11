using DevExpress.Maui.Core;
using MauiStudyApp.ViewModels;
using Microsoft.Maui.Layouts;

namespace MauiStudyApp.Views;

public partial class Page2 : ContentPage
{
    public Page2(Page2ViewModel page)
    {
        InitializeComponent();
        BindingContext = page;
    }

}
