using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Common;

public static class ConfigurationBuilderExtensions
{
    private const string Appsettings = "appsettings";

    public static IConfigurationBuilder AddAppSettingsFromJsons(
        this IConfigurationBuilder configuration,
        string? contour,
        string? path = null)
    {
        return configuration.AddConfigsFromJsons(
            Appsettings,
            contour,
            path);
    }

    public static IConfigurationBuilder AddMigratorSettingsFromJsons(
        this IConfigurationBuilder configuration,
        string? contour,
        string? path = null)
    {
        return configuration.AddConfigsFromJsons(
            "migrator",
            contour,
            path);
    }

    public static IConfigurationBuilder AddConfigsFromJsons(
        this IConfigurationBuilder configuration,
        string nameStart,
        string? contour,
        string? path = null)
    {
        path ??= Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "configs");

        if (!Directory.Exists(path)) return configuration;

        var files = Directory
            .GetFiles(
                path,
                "*.json",
                SearchOption.AllDirectories)
            .OrderBy(f => f);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (!ShouldLoad(
                    fileName,
                    nameStart,
                    contour))
                continue;

            configuration.AddJsonFile(
                file,
                true,
                false);
        }

        return configuration;
    }

    private static bool ShouldLoad(
        string fileName,
        string nameStart,
        string? contour)
    {
        var additionalName = string.IsNullOrWhiteSpace(contour) ? null : $".{contour}";
        return string.Equals(
                   fileName,
                   $"{nameStart}.json",
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   fileName,
                   $"{nameStart}{additionalName}.json",
                   StringComparison.OrdinalIgnoreCase);
    }
}