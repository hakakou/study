using CommunityToolkit.Maui;
using DevExpress.Maui;
using DevExpress.Maui.Core;
using MauiStudyApp.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiStudyApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        ThemeManager.ApplyThemeToSystemBars = true;
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseDevExpress(useLocalization: false)
            .UseDevExpressControls()
            .UseDevExpressCharts()
            .UseDevExpressTreeView()
            .UseDevExpressCollectionView()
            .UseDevExpressEditors()
            .UseDevExpressDataGrid()
            .UseDevExpressScheduler()
            .UseDevExpressGauges()
            .UseSkiaSharp()
            .RegisterAppServices()
            .RegisterViewModels()


        .ConfigureLifecycleEvents(l =>
        {
#if ANDROID
            l.AddAndroid(android =>
            {
                android.OnCreate((activity, bundle) => { });
            });
#elif IOS
            l.AddiOS(ios =>
            {
                ios.OpenUrl((app, url, options) => true);
            });
#endif
        })

            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                fonts.AddFont("roboto-regular.ttf", "Roboto");
            });

#if DEBUG
        builder.Logging.AddDebug()
            .AddFilter("Microsoft.Maui", LogLevel.Debug)
            .AddFilter("Microsoft.Maui.Controls", LogLevel.Debug)
            .AddFilter("Microsoft.Maui.Controls.Binding", LogLevel.Debug);
#endif

        //builder.Services.AddMvvm(options =>
        //{
        //    options.RegisterViewModelsFromAssemblyContaining<Page1ViewModel>();
        //    options.HostingModelType = BlazorHostingModelType.HybridMaui;
        //});

        return builder.Build();
    }

    static MauiAppBuilder RegisterViewModels(this MauiAppBuilder appBuilder)
    {
        appBuilder.Services.AddTransient<ViewModels.DefaultViewModel>();
        appBuilder.Services.AddTransient<ViewModels.Page1ViewModel>();
        appBuilder.Services.AddTransient<ViewModels.Page2ViewModel>();
        appBuilder.Services.AddTransient<ViewModels.Page3ViewModel>();
        appBuilder.Services.AddTransient<ViewModels.Page4ViewModel>();
        return appBuilder;
    }

    static MauiAppBuilder RegisterAppServices(this MauiAppBuilder appBuilder)
    {
        appBuilder.Services.AddSingleton<IDataService, Infrastructure.Services.DataService>();
        appBuilder.Services.AddSingleton<ICacheService, Infrastructure.Services.MemoryCacheService>();
        return appBuilder;
    }
}
