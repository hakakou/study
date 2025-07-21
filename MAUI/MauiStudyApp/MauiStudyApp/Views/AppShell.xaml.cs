namespace MauiStudyApp.Views
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Page1), typeof(Page1));
            Routing.RegisterRoute(nameof(Page2), typeof(Page2));
            Routing.RegisterRoute(nameof(Page3), typeof(Page3));
            Routing.RegisterRoute(nameof(Page4), typeof(Page4));
            Routing.RegisterRoute(nameof(Page5), typeof(Page5));
            Routing.RegisterRoute(nameof(Page6), typeof(Page6));
        }
    }
}
