using Microsoft.SemanticKernel;
using System.ComponentModel;
using OpenQA.Selenium;

public class LinkedinSearch(IWebDriver driver)
{
    [KernelFunction]
    [return: Description("Possible matches")]
    public async Task<IEnumerable<string>> Search(string name)
    {
        if (!driver.Url.StartsWith("https://www.linkedin.com/search/results/people/"))
        {
            driver.Navigate().GoToUrl("https://www.linkedin.com/search/results/people/?keywords=harry%20kakoulidis&origin=FACETED_SEARCH");
            await SeleniumUtils.Wait(1, 2);
        }

        await SeleniumUtils.Wait(7, 12);
        var searchInput = driver.FindElement(
            OpenQA.Selenium.By.CssSelector("input"));

        searchInput.SendKeys(OpenQA.Selenium.Keys.Control + "a");
        await SeleniumUtils.Wait(1, 2);

        searchInput.SendKeys(OpenQA.Selenium.Keys.Delete);
        await SeleniumUtils.Wait(1, 2);

        searchInput.SendKeys(name);
        await SeleniumUtils.Wait(4, 6);

        searchInput.SendKeys(OpenQA.Selenium.Keys.Enter);
        await SeleniumUtils.Wait(4, 8);

        var listItems = driver.FindElements(
            OpenQA.Selenium.By.CssSelector("div[role='listitem']"));

        var results = new List<string>();
        foreach (var item in listItems)
        {
            string text = item.Text;

            try
            {
                var link = item.FindElement(OpenQA.Selenium.By.TagName("a"));
                var href = link.GetAttribute("href");
                if (!string.IsNullOrEmpty(href))
                    text += $" | URL: {href}";
            }
            catch (NoSuchElementException) { }
            results.Add(text);
        }

        return results;
    }

    [KernelFunction]
    [return: Description("Text content of the profile")]
    public async Task<string> OpenProfile(string url)
    {
        driver.Navigate().GoToUrl(url);
        await SeleniumUtils.Wait(1, 3);
        await driver.ScrollDown(300, 1000);
        await SeleniumUtils.Wait(5, 10);

        try
        {
            var mainElement = driver.FindElement(OpenQA.Selenium.By.TagName("main"));
            return mainElement.Text;
        }
        catch (NoSuchElementException)
        {
            return "Main element not found on the page";
        }
    }
}
