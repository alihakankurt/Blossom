namespace Blossom.Utilities;

public static class StringExtensions
{
    public static string EndAt(this string text, int length)
    {
        return (text.Length > length) ? $"{text[..length]}..." : text;
    }
}
