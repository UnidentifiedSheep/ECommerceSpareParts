using System.Reflection;
using Api.Common.Extensions;
using Enums;
using FluentAssertions;
using LinqKit;
using Main.Application;
using Main.Application.Configs;
using Main.Entities;
using Main.Entities.Auth;
using Main.Entities.Product;

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
    [InlineData("tr")]
    public async Task All_LocalizableExceptions_Should_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var assembly = Assembly.GetAssembly(typeof(Product))!;

        await _localizationTests.TestLocalizableExceptions(assembly, localesPath, locale);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("tr")]
    public async Task All_AbstractValidators_Should_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var assembly = Assembly.GetAssembly(typeof(Global))!;

        await _localizationTests.TestAbstractValidatorLocalization(assembly, localesPath, locale);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("tr")]
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

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("tr")]
    public async Task All_Permissions_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        using var scoped = await _localizationTests.CreateLocalizer(localesPath, locale);
        scoped.SetLocale(locale);

        foreach (var permission in Enum.GetValues<PermissionCodes>())
        {
            var systemName = Permission.ToNormalizedPermission(permission);
            var nameKey = Permission.GetLocalizationNameKey(permission);
            var descriptionKey = Permission.GetLocalizationDescriptionKey(permission);
            
            scoped.TryGet(nameKey, out _).Should().BeTrue($"Missing key '{nameKey}' for permission '{systemName}'");
            scoped.TryGet(descriptionKey, out _).Should().BeTrue($"Missing key '{descriptionKey}' for permission '{systemName}'");
        }
    }
}
