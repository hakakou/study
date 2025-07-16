using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace MauiStudyApp.ViewModels;

public partial class Page4ViewModel : ObservableObject
{
    public Page4ViewModel(ILogger<Page4ViewModel> logger)
    {
        logger.LogInformation("Page4ViewModel created");
    }
}

public partial class AlertGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    string? alertText;

    [RelayCommand]
    public void GenerateAlert()
    {
        WeakReferenceMessenger.Default.Send(
            new AlertMessage(AlertText ?? "None"), "Performance");
    }

    [RelayCommand]
    public async Task RequestAlert()
    {
        // Example of using AsyncRequestMessage pattern
        string requestedAlert = await WeakReferenceMessenger.Default.Send(new InfoRequest());

        // Use the received alert
        AlertText = requestedAlert;
    }

}

public partial class ReplierViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<string> securityAlerts;

    public ReplierViewModel()
    {
        WeakReferenceMessenger.Default.Register<ReplierViewModel, InfoRequest>(this,
            (recipient, message) =>
        {
            message.Reply(recipient.GetAlertAsync());
        });
    }

    private async Task<string> GetAlertAsync()
    {
        // Simulate async operation (e.g., fetching from service)
        await Task.Delay(100);
        var latestAlert = SecurityAlerts?.LastOrDefault() ?? "No security alerts";
        return $"Security: {latestAlert}";
    }
}

public class InfoRequest : AsyncRequestMessage<string> { }

public class AlertMessage(string? value) : ValueChangedMessage<string?>(value);

