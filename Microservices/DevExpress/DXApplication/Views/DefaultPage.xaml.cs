using DevExpress.Maui.Core;

namespace DXApplication.Views
{
    public partial class DefaultPage : ContentPage
    {
        public DefaultPage()
        {
            InitializeComponent();
        }

        void OnClick(object sender, EventArgs e)
        {
            button.Content = button.ButtonType = (DXButtonType)(((int)button.ButtonType + 1) % 5);
        }
    }
}