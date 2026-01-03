using Core.Interfaces;
using Exceptions.Exceptions.Markups;
using Main.Core.Interfaces.DbRepositories;
using Main.Core.Interfaces.Pricing;
using Main.Core.Models;
using Serilog;

namespace Main.Application.Services.Pricing;

public class PriceSetup(
    IDefaultSettingsRepository defaultSettingsRepository,
    IMarkupRepository markupRepository,
    ICurrencyRepository currencyRepository,
    ICurrencyConverter currencyConverter,
    IPriceGenerator priceGenerator) : IPriceSetup
{
    private DefaultSettings _defaultSettings = new();

    public async Task SetupAsync(CancellationToken cancellationToken = default)
    {
        await defaultSettingsRepository.CreateDefaultSettingsIfNotExist(cancellationToken);
        _defaultSettings = await defaultSettingsRepository.GetDefaultSettingsAsync(cancellationToken);
        await SetupCurrencyConverterAsync(cancellationToken);
        if (_defaultSettings.SelectedMarkupId != -1)
        {
            await SetUserMarkupsAsync(cancellationToken);
            return;
        }

        await SetGeneratedMarkupsAsync(cancellationToken);
    }

    private async Task SetupCurrencyConverterAsync(CancellationToken cancellationToken = default)
    {
        var rates = await currencyRepository.GetCurrenciesToUsd(cancellationToken);
        currencyConverter.LoadRates(rates);
    }

    private async Task SetGeneratedMarkupsAsync(CancellationToken cancellationToken = default)
    {
        var generatedMarkup = await markupRepository.GetGeneratedMarkupsAsync(true, cancellationToken);
        Log.Logger.Warning("Generated markup not found.");
        if (generatedMarkup == null) return;
        priceGenerator.SetUp(generatedMarkup, _defaultSettings);
    }

    private async Task SetUserMarkupsAsync(CancellationToken cancellationToken = default)
    {
        var generatedMarkup =
            await markupRepository.GetMarkupByIdAsync(_defaultSettings.SelectedMarkupId, true, cancellationToken)
            ?? throw new MarkupGroupNotFoundException(_defaultSettings.SelectedMarkupId);
        priceGenerator.SetUp(generatedMarkup, _defaultSettings);
    }
}