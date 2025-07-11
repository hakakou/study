using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;


namespace MauiStudyApp.ViewModels;

public partial class DefaultViewModel : ObservableObject
{
    //readonly IMvvmNavigationManager _navManager;
    public ObservableCollection<PageItem> Pages { get; }

    public DefaultViewModel()
    {
        Pages = new ObservableCollection<PageItem>
        {
            new PageItem { Title = "Page1", Route = typeof(Page1ViewModel) },
            new PageItem { Title = "Page2", Route = typeof(Page2ViewModel) },
            new PageItem { Title = "Page3", Route = typeof(Page3ViewModel) },
            new PageItem { Title = "Page4", Route = typeof(Page4ViewModel) }
        };
    }

    [RelayCommand]
    private async Task OpenPageAsync(PageItem? item)
    {
        await Shell.Current.GoToAsync(item.Title);
    }
}

public class PageItem
{
    public string Title { get; set; } = string.Empty;
    public Type Route { get; set; }
}

