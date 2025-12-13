public static class SeleniumUtils
{
    private static readonly Random rnd = new Random();

    public static async Task Wait(int minSec, int maxSec)
    {
        int delay = rnd.Next(minSec * 1000, maxSec * 1000);
        await Task.Delay(delay);
    }
}