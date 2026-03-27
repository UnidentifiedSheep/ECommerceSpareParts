using System.Reflection;

namespace Api.Common.Extensions;

public static class AssemblyExtensions
{
    public static string GetDefaultLocalizationPath(this Assembly assembly)
    {
        return Path.Combine(Path.GetDirectoryName(assembly.Location)!, "Localization");
    }
}