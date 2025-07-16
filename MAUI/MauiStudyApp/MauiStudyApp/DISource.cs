namespace MauiStudyApp.Behaviors;

public class DISource : IMarkupExtension
{
    public Type Type { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        var app = Application.Current;
        var window = app?.Windows?.Count > 0 ? app.Windows[0] : null;
        var page = window?.Page;
        var mauiContext = page?.Handler?.MauiContext;

        if (mauiContext?.Services != null)
        {
            return mauiContext.Services.GetRequiredService(Type)!;
        }

        return serviceProvider.GetRequiredService(Type);
    }
}


[ContentProperty("Text")]
public class TranslateExtension : IMarkupExtension<string>
{
    public string Text { get; set; }
    public string ProvideValue(IServiceProvider serviceProvider)
    {
        return Text + "GAGA";
    }
    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return (this as IMarkupExtension<string>).ProvideValue(serviceProvider);
    }
}