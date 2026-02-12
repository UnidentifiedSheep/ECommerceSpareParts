using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Exceptions.Exceptions.Markups;
using Microsoft.Extensions.Logging;
using Pricing.Abstractions.Constants;
using Pricing.Abstractions.Interfaces.DbRepositories;
using Pricing.Abstractions.Interfaces.Services.Pricing;

namespace Pricing.Application.Services.ArticlePricing;

public class MarkupSetup(IMarkupRepository markupRepository, ISettingsContainer settingsContainer, 
    IMarkupService markupService, ILogger<MarkupSetup> logger) : IMarkupSetup
{

    public async Task SetupAsync(CancellationToken cancellationToken = default)
    {
        var setting = settingsContainer.GetSetting(Settings.Pricing);
        
        if (setting.SelectedMarkupId != -1)
        {
            await SetUserMarkupsAsync(setting.SelectedMarkupId, cancellationToken);
            return;
        }

        await SetGeneratedMarkupsAsync(cancellationToken);
    }


    private async Task SetGeneratedMarkupsAsync(CancellationToken cancellationToken = default)
    {
        var generatedMarkup = await markupRepository.GetGeneratedMarkupsAsync(true, cancellationToken);
        if (generatedMarkup == null)
        {
            if (logger.IsEnabled(LogLevel.Warning)) 
                logger.LogWarning("Не удалось найти сгенерированную группу наценок.");
            return;
        }
        markupService.SetUp(generatedMarkup);
    }

    private async Task SetUserMarkupsAsync(int id, CancellationToken cancellationToken = default)
    {
        var generatedMarkup = await markupRepository.GetMarkupByIdAsync(id, false, cancellationToken)
                              ?? throw new MarkupGroupNotFoundException(id);
        markupService.SetUp(generatedMarkup);
    }
}