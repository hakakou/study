using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiApp4.ViewModels
{
    public partial class DragDropDemoViewModel : ObservableObject
    {
        [ObservableProperty]
        private string dropZone1Content = "Drop items here";

        [ObservableProperty]
        private string dropZone2Content = "Drop items here";

        [ObservableProperty]
        private string dropZone3Content = "Drop items here";

        [ObservableProperty]
        private string statusMessage = "Ready to drag and drop";
    }
}
