using System.Reflection;
using Api.Common.Extensions;
using Main.Abstractions.Constants;
using Main.Application.Configs;
using Main.Entities;

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
    
    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    public async Task All_DbValidators_Should_Have_Valid_Localization(string locale)
    {
        ValidationConfiguration.Configure();
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var constants = typeof(ValidationFunctions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => f.GetValue(null))
            .Cast<string>()
            .ToList();
        
        await _localizationTests.TestDbValidatorLocalization(constants, localesPath, locale);
    }
}