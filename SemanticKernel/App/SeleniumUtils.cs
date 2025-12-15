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

    public static async Task NaturalScrollDown(this IWebDriver driver, int minSec, int maxSec)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

        // Scroll to top first
        js.ExecuteScript("window.scrollTo(0, 0);");
        await Task.Delay(500);

        int totalDuration = rnd.Next(minSec * 1000, maxSec * 1000);
        int scrollSteps = rnd.Next(5, 12); // Random number of scroll actions
        int averageStepDuration = totalDuration / scrollSteps;

        for (int i = 0; i < scrollSteps; i++)
        {
            // Random scroll amount (sometimes small, sometimes larger)
            int scrollAmount = rnd.Next(200, 800);

            // Occasionally scroll up a bit (more natural)
            if (rnd.Next(0, 100) < 15) // 15% chance to scroll up
            {
                scrollAmount = -rnd.Next(50, 200);
            }

            js.ExecuteScript($"window.scrollBy(0, {scrollAmount});");

            // Random pause between scrolls (varying speeds)
            int pauseDuration = rnd.Next(averageStepDuration / 2, averageStepDuration * 3 / 2);
            await Task.Delay(pauseDuration);
        }

        await Task.Delay(500);
    }
}