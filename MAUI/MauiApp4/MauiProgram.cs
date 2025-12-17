using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace MauiApp4
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            //// Register pages and view models
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ViewModels.MainViewModel>();

            builder.Services.AddTransient<CustomerEditPage>();
            builder.Services.AddTransient<ViewModels.CustomerEditViewModel>();
            
            builder.Services.AddTransient<DragDropDemoPage>();
            builder.Services.AddTransient<ViewModels.DragDropDemoViewModel>();

            return builder.Build();
        }
    }
}
