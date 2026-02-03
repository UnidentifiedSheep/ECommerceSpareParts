using Core.Interfaces;
using Exceptions.Exceptions.Markups;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Models;
using Serilog;

namespace Main.Application.Services.ArticlePricing;

public class MarkupSetup(
    IDefaultSettingsRepository defaultSettingsRepository,
    IMarkupRepository markupRepository,
    ICurrencyRepository currencyRepository,
    ICurrencyConverter currencyConverter,
    IMarkupService markupService) : IMarkupSetup
{
    private Settings _settings = new();

    public async Task SetupAsync(CancellationToken cancellationToken = default)
    {
        await defaultSettingsRepository.CreateDefaultSettingsIfNotExist(cancellationToken);
        _settings = await defaultSettingsRepository.GetDefaultSettingsAsync(cancellationToken);
        await SetupCurrencyConverterAsync(cancellationToken);
        if (_settings.SelectedMarkupId != -1)
        {
            await SetUserMarkupsAsync(cancellationToken);
            return;
        }

        await SetGeneratedMarkupsAsync(cancellationToken);
    }
    //TODO: move to other service

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
        markupService.SetUp(generatedMarkup, _settings);
    }

    private async Task SetUserMarkupsAsync(CancellationToken cancellationToken = default)
    {
        var generatedMarkup =
            await markupRepository.GetMarkupByIdAsync(_settings.SelectedMarkupId, true, cancellationToken)
            ?? throw new MarkupGroupNotFoundException(_settings.SelectedMarkupId);
        markupService.SetUp(generatedMarkup, _settings);
    }
}