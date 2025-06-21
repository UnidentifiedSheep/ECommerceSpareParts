using System.Diagnostics;

namespace Core.StaticFunctions;

public static class ConcurrencyStatic
{
    public static string GetConcurrencyCode(params object[] args)
    {
        var hc = new HashCode();
        foreach (var arg in args) hc.Add(arg);
        return hc.ToHashCode().ToString("X8");;
    }

}