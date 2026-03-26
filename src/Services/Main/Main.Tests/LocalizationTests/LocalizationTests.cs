using System.Reflection;
using Api.Common.Extensions;
using Main.Abstractions.Utils;

namespace Tests.LocalizationTests;

public class LocalizationTests
{
    private readonly Test.Common.Tests.LocalizationTests _localizationTests;

    public LocalizationTests()
    {
        _localizationTests = new Test.Common.Tests.LocalizationTests();
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    public async Task All_LocalizableExceptions_Should_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var assembly = Assembly.GetAssembly(typeof(CacheKeys))!;

        await _localizationTests.TestLocalizableExceptions(assembly, localesPath, locale);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    public async Task All_AbstractValidators_Should_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var assembly = Assembly.GetAssembly(typeof(Main.Application.Global))!;

        await _localizationTests.TestAbstractValidatorLocalization(assembly, localesPath, locale);
    }
}