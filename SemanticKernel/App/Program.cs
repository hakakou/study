using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Logging;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

Console.Clear();
Console.OutputEncoding = System.Text.Encoding.UTF8;
Conf.Init<Program>();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole().SetMinimumLevel(MsLogLevel.Error);
builder.Logging.AddFile("c:\\Unzip\\app\\app-{Date}.txt", MsLogLevel.Trace);

// Register Semantic Kernel services
builder.Services
    .AddKernel()
    .TavilyTextSearch()
    .DefaultChatCompletion();

// Register Chrome WebDriver
builder.Services.AddSingleton<IWebDriver>(sp =>
{
    var cm = new ChromeOptions();
    cm.AddArgument(@"user-data-dir=c:/Unzip/brave-profile");
    var service = ChromeDriverService.CreateDefaultService();
    var driver = new ChromeDriver(service, cm);
    driver.Navigate().GoToUrl("https://www.linkedin.com/search/results/people/?keywords=harry%20kakoulidis&origin=FACETED_SEARCH");
    return driver;
});

// Register the main application service
builder.Services.AddSingleton<App>();

var host = builder.Build();

try
{
    var app = host.Services.GetRequiredService<App>();
    await app.Execute();
}
finally
{
    var driver = host.Services.GetRequiredService<IWebDriver>();
    driver.Quit();
    await host.StopAsync();
}
