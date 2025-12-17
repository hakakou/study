namespace MauiApp4
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register route for CustomerEditPage
            Routing.RegisterRoute(nameof(CustomerEditPage), typeof(CustomerEditPage));
            Routing.RegisterRoute(nameof(CustomerEditPage), typeof(CustomerEditPage));
            Routing.RegisterRoute(nameof(DragDropDemoPage), typeof(DragDropDemoPage));
        }
    }
}
