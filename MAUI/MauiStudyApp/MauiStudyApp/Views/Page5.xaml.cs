using MauiStudyApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MauiStudyApp.Views
{
    public partial class Page5 : ContentPage
    {
        public Page5(Page5ViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;



                var template = (DataTemplate)Application.Current.Resources["customerTemplate"];
        }
    }
}
