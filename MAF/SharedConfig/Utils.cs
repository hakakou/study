using System.Text.Json;

public static class Utils
{
    private static readonly JsonSerializerOptions s_jsonOptionsCache = new() { WriteIndented = true };
    private static readonly object s_lock = new object();

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, s_jsonOptionsCache);
    }
}