using Microsoft.Extensions.DependencyInjection;

namespace MauiApp4
{
    public partial class App : Application
    {
        public App()
        {
            //  necessary for initializing SQLitePCLRaw on iOS devices.
            SQLitePCL.Batteries_V2.Init();
            using var context = new CrmContext();
            context.Database.EnsureCreated();
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