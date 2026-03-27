using System.Text.Json;
using FluentAssertions;
using Localization.Abstractions.Models;
using Localization.Domain;

namespace Localization.Unit.Tests;

public class JsonLocalizerContainerLoaderTests
{
    [Fact]
    public async Task LoadAsync_ShouldLoadFilesRecursively()
    {
        var locale = "en";
        var keyValues = new Dictionary<string, string>
        {
            ["Test.Key"] = "value"
        };
        using var localeFiles = await TempLocaleFile.Create(locale, keyValues);

        var container = new LocalizerContainer(locale);

        var loader = new JsonLocalizerContainerLoader(localeFiles.BaseDir);

        await loader.LoadAsync([container]);

        container.KetMessages["Test.Key"].Should().Be("value");
    }

    [Fact]
    public async Task LoadAsync_ShouldIgnoreUnknownLocales()
    {
        var locale = "fr";
        var keyValues = new Dictionary<string, string>
        {
            ["key"] = "value"
        };
        using var localeFiles = await TempLocaleFile.Create(locale, keyValues);

        var container = new LocalizerContainer("en");

        var loader = new JsonLocalizerContainerLoader(localeFiles.BaseDir);

        await loader.LoadAsync([container]);

        container.KetMessages.Should().BeEmpty();
    }

    private class TempLocaleFile : IDisposable
    {
        public readonly string BaseDir;

        private TempLocaleFile(string baseDir)
        {
            BaseDir = baseDir;
        }

        public void Dispose()
        {
            if (Directory.Exists(BaseDir))
                Directory.Delete(BaseDir, true);
        }

        public static async Task<TempLocaleFile> Create(string locale, Dictionary<string, string> keyValues)
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);

            var subDir = Path.Combine(dir, "sub");
            Directory.CreateDirectory(subDir);

            var filePath = Path.Combine(subDir, $"{locale}.json");

            var model = new LocaleFullInfoModel
            {
                Locale = locale,
                KeyValues = keyValues
            };

            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(model));
            return new TempLocaleFile(dir);
        }
    }
}