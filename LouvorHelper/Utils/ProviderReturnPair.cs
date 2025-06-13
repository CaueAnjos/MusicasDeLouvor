namespace LouvorHelper.Utils;

internal static class ProviderReturnPair
{
    public static KeyValuePair<string, string?> ReturnPair(string label, string? lyrics = null)
    {
        return new KeyValuePair<string, string?>(label, lyrics);
    }
}
