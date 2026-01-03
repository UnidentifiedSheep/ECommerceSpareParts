namespace Core.Attributes;

public abstract class BaseSigner
{
    public static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static byte[] Base64UrlDecode(string str)
    {
        str = str.Replace('-', '+').Replace('_', '/');
        switch (str.Length % 4)
        {
            case 2: str += "=="; break;
            case 3: str += "="; break;
        }
        return Convert.FromBase64String(str);
    }
}