using System.Reflection;

namespace Api.Common.Extensions;

public static class ConfigurationBuilderExtensions
{
    private const string Appsettings = "appsettings";

    public static IConfigurationBuilder AddAppSettingsFromJsons(
        this IConfigurationBuilder configuration,
        string? contour,
        string? path = null)
        => configuration.AddConfigsFromJsons(Appsettings, contour, path);
    
    public static IConfigurationBuilder AddMigratorSettingsFromJsons(
        this IConfigurationBuilder configuration,
        string? contour,
        string? path = null)
        => configuration.AddConfigsFromJsons("migrator", contour, path);
    
    public static IConfigurationBuilder AddConfigsFromJsons(
        this IConfigurationBuilder configuration,
        string nameStart,
        string? contour,
        string? path = null)
    {
        path ??= Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "configs");

        if (!Directory.Exists(path)) return configuration;

        var additionalName = string.IsNullOrWhiteSpace(contour) ? null : $".{contour}";
        var appsettingsFileName = $"{Appsettings}{additionalName}.json";

        var files = Directory
            .GetFiles(path, "*.json", SearchOption.AllDirectories)
            .OrderBy(f => f);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (fileName.StartsWith(nameStart) && fileName != appsettingsFileName)
                continue;

            configuration.AddJsonFile(file, true, true);
        }

        return configuration;
    }
}