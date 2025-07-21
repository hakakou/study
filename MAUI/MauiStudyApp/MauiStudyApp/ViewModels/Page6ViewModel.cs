using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiStudyApp.ViewModels;

public partial class Page6ViewModel : ObservableObject
{
    // Empty ViewModel for Page6
}


public class ThemeInfo
{
    public AppTheme AppTheme { get; }
    public string Caption { get; }
    public ThemeInfo(AppTheme theme, string caption)
    {
        AppTheme = theme;
        Caption = caption;
    }
}

public partial class ThemeSettings : ObservableObject
{
    public static List<ThemeInfo> ThemesList { get; } = new List<ThemeInfo>() {
        new ThemeInfo(AppTheme.Unspecified, "System"),
        new ThemeInfo(AppTheme.Light, "Light"),
        new ThemeInfo(AppTheme.Dark, "Dark")
        };
    public static ThemeSettings Current { get; } = new ThemeSettings();

    [ObservableProperty]
    public ThemeInfo selectedTheme = ThemesList.First();

    partial void OnSelectedThemeChanged(ThemeInfo oldValue, ThemeInfo newValue)
    {
        Application.Current.UserAppTheme = newValue.AppTheme;
    }
}