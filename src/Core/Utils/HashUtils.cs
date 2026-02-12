namespace Utils;

public static class HashUtils
{
    public static string ComputeHash(params object[] args)
    {
        var hc = new HashCode();
        foreach (var arg in args) hc.Add(arg);
        return hc.ToHashCode().ToString("X8");
    }
}