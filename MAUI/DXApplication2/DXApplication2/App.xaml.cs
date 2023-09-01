using DXApplication2.Views;

namespace DXApplication2
{
    public partial class App : Application
    {
        public App()
        {
            using var entitiesContext = new Infrastructure.Data.EntitiesContext();
            SQLitePCL.Batteries_V2.Init();
            entitiesContext.Database.EnsureCreated();
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}