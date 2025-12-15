using Microsoft.Extensions.DependencyInjection;

namespace MauiApp4
{
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                // Android.Runtime.JavaProxyThrowable logs here before fatal crash
                System.Diagnostics.Debug.WriteLine("Unhandled: " + e.ExceptionObject);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Unobserved: " + e.Exception);
                e.SetObserved();
            };

            //  necessary for initializing SQLitePCLRaw on iOS devices.
            SQLitePCL.Batteries_V2.Init();
            using var context = new CrmContext();
            context.Database.EnsureCreated();

            if (!context.Customers.Any())
            {
                context.Customers.Add(new Customer
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                });
                context.SaveChanges();
            }

            Task.Run(async () => await CopyToAppDataDirectory("AboutAssets.txt"));
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        public async Task<string> CopyToAppDataDirectory(string filename)
        {
            string targetFile = Path.Combine(
                FileSystem.Current.AppDataDirectory, filename);

            if (!File.Exists(targetFile))
            {
                using Stream inputStream =
                    await FileSystem.Current.OpenAppPackageFileAsync(filename);
                using FileStream outputStream = File.Create(targetFile);
                await inputStream.CopyToAsync(outputStream);
            }

            return targetFile;
        }
    }
}