using DevExpress.Maui.Core;
using MauiStudyApp.ViewModels;
using Microsoft.Maui.Layouts;

namespace MauiStudyApp.Views;

public partial class Page1 : ContentPage
{
    public Page1(Page1ViewModel page) 
    {
        InitializeComponent();
        BindingContext = page;
    }

}
