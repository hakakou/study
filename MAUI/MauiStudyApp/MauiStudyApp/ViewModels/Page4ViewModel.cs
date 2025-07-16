using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;

namespace MauiStudyApp.ViewModels;

public partial class Page4ViewModel : ObservableObject
{
}

public partial class AlertGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    string? alertText;
    
    [RelayCommand]
    public void GenerateAlert()
    {
        string channelType = ++alertCount % 2 == 0 ?
            AlertTypes.Security : AlertTypes.Performance;
        WeakReferenceMessenger.Default.Send(
            new AlertMessage(AlertText ?? "None"), channelType);
    }
    
    int alertCount = 0;
}

public partial class PerformanceMonitorViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<string> performanceAlerts;

    public PerformanceMonitorViewModel()
    {
        performanceAlerts = new ObservableCollection<string>();
        WeakReferenceMessenger.Default.Register<AlertMessage, string>(this, AlertTypes.Performance, (r, alert) =>
        {
            PerformanceAlerts.Add(alert.Value);
        });
    }
}

public partial class SecurityMonitorViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<string> securityAlerts;

    public SecurityMonitorViewModel()
    {
        securityAlerts = new ObservableCollection<string>();
        WeakReferenceMessenger.Default.Register<AlertMessage, string>(this, AlertTypes.Security, (r, alert) =>
        {
            SecurityAlerts.Add(alert.Value);
        });
    }
}

public class RequestAlert : RequestMessage<string> { }

public static class AlertTypes
{
    public static string Security = "SecurityAlert";
    public static string Performance = "Performance";
}

public class AlertMessage(string? value) : ValueChangedMessage<string?>(value);
