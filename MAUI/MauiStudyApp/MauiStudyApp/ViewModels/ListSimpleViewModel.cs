using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiStudyApp.ViewModels
{
    public partial class ListSimpleViewModel : ObservableObject
    {

        [ObservableProperty]
        private IEnumerable<SimpleItem> items;

        public ListSimpleViewModel()
        {
            var names = new string[] { "Robert King", "Nancy Davolio", "Michael Suyama", "Steven Buchanan", "Margaret Peacock", "Andrew Fuller" };
            var phones = new string[] { "(71) 55-4848", "(206) 555-9857", "(71) 555-7773", "(71) 555-4848", "(206) 555-8122", "(206) 555-9482" };
            Items = Enumerable.Range(0, names.Length).Select(i => new SimpleItem
            {
                Name = names[i],
                Description = phones[i]
            }).ToList();
        }

        [RelayCommand]
        private async Task HandleActionAsync(SimpleItem item)
        {
            await Shell.Current.DisplayAlert(item.Name, "Action executed", "OK");
        }
    }

    public class SimpleItem
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}