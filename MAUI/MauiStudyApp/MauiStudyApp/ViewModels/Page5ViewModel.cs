using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MauiStudyApp.ViewModels
{
    public partial class Page5ViewModel : ObservableObject
    {
        public ObservableCollection<Customer2> Customers { get; } = new();
    }

    public class Customer2
    {
        public string CustomerName { get; set; }
    }
}
