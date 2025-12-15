namespace MauiApp4
{
    public partial class CustomerEditPage : ContentPage
    {
        public CustomerEditPage(ViewModels.CustomerEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
