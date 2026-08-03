#!/usr/bin/env dotnet

using System.Xml.Linq;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: DiscoverChangedProjects.cs <affected.txt>");
    return 2;
}

var affectedFilePath = Path.GetFullPath(args[0]);

if (!File.Exists(affectedFilePath))
{
    Console.Error.WriteLine($"Affected projects file not found: {affectedFilePath}");
    return 1;
}

foreach (var projectPath in File
             .ReadLines(affectedFilePath)
             .Where(path => !string.IsNullOrWhiteSpace(path))
             .Select(path => Path.GetFullPath(path.Trim()))
             .Distinct(StringComparer.Ordinal)
             .OrderBy(path => path, StringComparer.Ordinal))
{
    var projectType = GetProjectType(projectPath);

    if (projectType is ProjectType.Api or ProjectType.Worker or ProjectType.Migrator)
        Console.WriteLine(projectPath);
}

return 0;

static ProjectType GetProjectType(string projectPath)
{
    var document = XDocument.Load(projectPath);

    var sdk = document.Root?
        .Attribute("Sdk")?
        .Value;

    var outputType = document
        .Descendants("OutputType")
        .Select(element => element.Value.Trim())
        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    if (string.Equals(outputType, "Library", StringComparison.OrdinalIgnoreCase))
        return ProjectType.Library;

    if (string.Equals(outputType, "Exe", StringComparison.OrdinalIgnoreCase) &&
        Path.GetFileNameWithoutExtension(projectPath)
            .EndsWith(".Migrator", StringComparison.OrdinalIgnoreCase))
        return ProjectType.Migrator;

    return sdk switch
    {
        "Microsoft.NET.Sdk.Web" => ProjectType.Api,
        "Microsoft.NET.Sdk.Worker" => ProjectType.Worker,
        _ => ProjectType.Unknown
    };
}

enum ProjectType
{
    Unknown,
    Api,
    Worker,
    Migrator,
    Library
}
