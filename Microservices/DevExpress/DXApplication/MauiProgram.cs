using DevExpress.Maui;
using DevExpress.Maui.Core;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace DXApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            ThemeManager.ApplyThemeToSystemBars = true;
            var builder = MauiApp.CreateBuilder()
                .UseMauiApp<App>()
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                    fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                    fonts.AddFont("roboto-regular.ttf", "Roboto");
                });
            return builder.Build();
        }

    }
}
