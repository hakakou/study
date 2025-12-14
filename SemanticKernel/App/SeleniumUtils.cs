using OpenQA.Selenium;

public static class SeleniumUtils
{
    private static readonly Random rnd = new Random();

    public static async Task Wait(int minSec, int maxSec)
    {
        int delay = rnd.Next(minSec * 1000, maxSec * 1000);
        await Task.Delay(delay);
    }

    public static async Task ScrollDown(this IWebDriver driver, int minPixels, int maxPixels)
    {
        int scrollAmount = rnd.Next(minPixels, maxPixels);
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        js.ExecuteScript($"window.scrollBy(0, {scrollAmount});");
        await Task.Delay(100); // Small delay after scrolling
    }
}