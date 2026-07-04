namespace Localization.Abstractions;

public static class LocalizedMessageFormatter
{
    public static bool TryFormat(
        string template,
        object[]? arguments,
        out string result)
    {
        result = template;
        if (arguments == null || arguments.Length == 0) return true;

        try
        {
            result = string.Format(template, arguments);
            return true;
        }
        catch (FormatException)
        {
            result = $"{result} [Error formatting message]";
            return false;
        }
    }
}