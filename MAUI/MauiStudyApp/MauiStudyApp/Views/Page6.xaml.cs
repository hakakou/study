using Microsoft.Maui.Controls;

namespace MauiStudyApp.Views
{
    public partial class Page6 : ContentPage
    {
        public Page6()
        {
            InitializeComponent();
            BindingContext = new MauiStudyApp.ViewModels.Page6ViewModel();
        }
    }
}
