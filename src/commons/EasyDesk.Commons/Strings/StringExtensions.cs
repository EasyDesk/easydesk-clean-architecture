namespace EasyDesk.Commons.Strings;

public static class StringExtensions
{
    public static string RemovePrefix(this string text, string prefix)
    {
        if (text.StartsWith(prefix))
        {
            return text[prefix.Length..];
        }
        return text;
    }

    public static string RemoveSuffix(this string text, string suffix)
    {
        if (text.EndsWith(suffix))
        {
            return text[..^suffix.Length];
        }
        return text;
    }
}
