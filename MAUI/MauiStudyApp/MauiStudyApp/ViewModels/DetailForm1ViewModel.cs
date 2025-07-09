using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiStudyApp.ViewModels
{
    public partial class DetailForm1ViewModel : ObservableObject
    {

        [ObservableProperty]
        private string fullName = "Alfred Newman";

        [ObservableProperty]
        private string phoneNumber = "(650) 565-1234";

        [ObservableProperty]
        private string email = "alfred.nm@newmansystems.com";

        [ObservableProperty]
        private string address = "900 Newman Center";

        [ObservableProperty]
        private string city = "New York";

        [ObservableProperty]
        private string company = "Newman Systems";

        public string NameInitials => string.Concat(FullName.Split(' ').Select(s => s[0]));


        [RelayCommand]
        private async Task HandleActionAsync()
        {
            await Shell.Current.DisplayAlert("Action", "Action executed", "OK");
        }
    }
}