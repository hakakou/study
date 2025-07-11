using DevExpress.Maui.Core;
using MauiStudyApp.ViewModels;
using Microsoft.Maui.Layouts;

namespace MauiStudyApp.Views;

public partial class Page3 : ContentPage
{
    public Page3(Page3ViewModel page)
    {
        InitializeComponent();
        BindingContext = page;
    }

}
