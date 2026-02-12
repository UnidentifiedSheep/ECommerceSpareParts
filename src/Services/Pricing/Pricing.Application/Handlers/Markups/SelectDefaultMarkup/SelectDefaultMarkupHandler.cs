using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Markup;
using Exceptions.Exceptions.Markups;
using MassTransit;
using MediatR;
using Pricing.Abstractions.Interfaces.DbRepositories;

namespace Pricing.Application.Handlers.Markups.SelectDefaultMarkup;

[Transactional]
public record SelectDefaultMarkupCommand(int MarkupGroupId) : ICommand;

public class SelectDefaultMarkupHandler(ISettingsService settingsService, ISettingsContainer settingsContainer, IPublishEndpoint publishEndpoint,
    IMarkupRepository markupRepository, IUnitOfWork unitOfWork) : ICommandHandler<SelectDefaultMarkupCommand>
{
    public async Task<Unit> Handle(SelectDefaultMarkupCommand request, CancellationToken cancellationToken)
    {
        var setting = settingsContainer.GetSetting(Abstractions.Constants.Settings.Pricing);
        int selectedMarkupId = setting.SelectedMarkupId;
        if (selectedMarkupId == request.MarkupGroupId) return Unit.Value;

        if (request.MarkupGroupId == -1)
            selectedMarkupId = -1;
        else
        {
            var markupGroup = await markupRepository.GetMarkupByIdAsync(request.MarkupGroupId, true, cancellationToken)
                              ?? throw new MarkupGroupNotFoundException(request.MarkupGroupId);
            selectedMarkupId = markupGroup.Id;
        }

        var settingWithNewMarkup = setting with { SelectedMarkupId = selectedMarkupId };
        await settingsService.SetSetting(Abstractions.Constants.Settings.Pricing, settingWithNewMarkup, cancellationToken);
        
        await publishEndpoint.Publish(new MarkupGroupChangedEvent { GroupId = request.MarkupGroupId }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}