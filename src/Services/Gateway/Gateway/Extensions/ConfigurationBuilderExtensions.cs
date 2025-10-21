namespace Gateway.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonFromDirectory(this IConfigurationBuilder builder, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return builder;

        var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);

        foreach (var jsonFile in jsonFiles)
            builder.AddJsonFile(jsonFile, optional: false, reloadOnChange: true);
        

        return builder;
    }
}