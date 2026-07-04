using System.Reflection;
using Analytics.Application.NamedObjects.Metrics;
using Analytics.Application.NamedObjects.Metrics.MetricInputValidators;
using Analytics.Entities.Metrics;
using Api.Common.Extensions;
using Application.Common.Interfaces.NamedObject;
using FluentAssertions;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Integration.Tests;

public class LocalizationTests : global::Tests.Tests.LocalizationTests
{
    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("tr")]
    public async Task All_LocalizableExceptions_Should_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var assembly = Assembly.GetAssembly(typeof(Metric))!;

        await TestLocalizableExceptions(
            assembly,
            localesPath,
            locale);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("tr")]
    public async Task All_AbstractValidators_Should_Have_Valid_Localization(string locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var assembly = Assembly.GetAssembly(typeof(MetricInputBaseValidator))!;

        await TestAbstractValidatorLocalization(
            assembly,
            localesPath,
            locale);
    }

    [Theory]
    [InlineData("ru")]
    [InlineData("en")]
    [InlineData("tr")]
    public async Task All_MetricDefinitions_Should_Have_Valid_Localization(string locale)
    {
        using var localizer = await CreateLocalizer(locale);

        var sp = new ServiceProviderBuilder()
            .Build(
                new ServiceProviderArguments
                {
                    PgsqlConnectionString = "",
                    CacheConnectionString = ""
                });

        var registry = sp.GetRequiredService<INamedObjectRegistry<MetricDefinitionNamedObjectBase>>();

        foreach (var definition in registry.All)
        {
            localizer.TryGet(definition.NameLocalizationKey, out _)
                .Should()
                .BeTrue(
                    $"Missing key '{definition.NameLocalizationKey}' " +
                    $"for metric definition '{definition.MetricType}'");
            localizer.TryGet(definition.DescriptionLocalizationKey, out _)
                .Should()
                .BeTrue(
                    $"Missing key '{definition.DescriptionLocalizationKey}' " +
                    $"for metric '{definition.MetricType}'");
        }
    }

    private async Task<IScopedStringLocalizer> CreateLocalizer(Locale locale)
    {
        var localesPath = Assembly.GetExecutingAssembly().GetDefaultLocalizationPath();
        var scoped = await CreateLocalizer(localesPath, locale);
        scoped.SetLocale(locale);
        return scoped;
    }
}