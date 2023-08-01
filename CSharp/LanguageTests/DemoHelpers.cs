namespace Haka.Debug;

public static class Extensions
{
    public static string Serialize(this object obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj,
           new System.Text.Json.JsonSerializerOptions
           {
               WriteIndented = true
           });
    }
}