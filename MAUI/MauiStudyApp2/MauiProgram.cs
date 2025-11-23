using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiStudyApp2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<CanvasRequest>();
        builder.Services.AddTransient<Page3>();

#if DEBUG
        builder.Configuration.AddUserSecrets<App>();
#endif

        var key = builder.Configuration["OpenAI:ApiKey"];

        builder.Services.AddOpenAIChatClient(
            modelId: "gpt-5-nano-2025-08-07",
            apiKey: key);

        var kernelBuilder = builder.Services.AddKernel();
        kernelBuilder.Plugins.AddFromType<Drawer>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
