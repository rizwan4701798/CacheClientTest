namespace TestApp.Helpers;

public static class Utils
{
    public static int ExtractIdFromKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return 0;
        
        var parts = key.Split(':');
        if (parts.Length > 1 && int.TryParse(parts[^1], out var id))
            return id;
        return 0;
    }
}
