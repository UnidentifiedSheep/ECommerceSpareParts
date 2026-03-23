using System.Collections.Concurrent;
using System.Text.Json;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Localization.Domain;

public class JsonLocalizerContainerLoader(string dirPath) : ILocalizerContainerLoader
{
    public async Task LoadAsync(IEnumerable<ILocalizerContainer> containers)
    {
        var containersDict = containers
            .ToDictionary(c => c.Locale, c => c);

        if (containersDict.Count == 0) return;

        var localesValues = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        var files = Directory.EnumerateFiles(
            dirPath,
            "*.json",
            SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (file, _) =>
        {
            var model = await ReadFile(file);
            var locale = model.Locale.ToUpperInvariant();

            if (!containersDict.ContainsKey(locale))
                return;

            var dict = localesValues.GetOrAdd(locale, _ => new ConcurrentDictionary<string, string>());

            foreach (var (key, value) in model.KeyValues)
                dict.TryAdd(key, value);
            
        });

        foreach (var (locale, values) in localesValues)
        {
            var container = containersDict[locale];
            container.Initialize(values.ToDictionary());
        }
    }

    private async Task<LocaleFullInfoModel> ReadFile(string path)
    {
        await using var stream = File.OpenRead(path);

        var model = await JsonSerializer.DeserializeAsync<LocaleFullInfoModel>(stream);

        ArgumentNullException.ThrowIfNull(model);
        return model;
    }
}