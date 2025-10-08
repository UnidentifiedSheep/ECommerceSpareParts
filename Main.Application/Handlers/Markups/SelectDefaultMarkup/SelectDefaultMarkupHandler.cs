using Core.Attributes;
using Core.Contracts;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Markups;
using Main.Application.Interfaces;
using MediatR;

namespace Main.Application.Handlers.Markups.SelectDefaultMarkup;

[Transactional]
public record SelectDefaultMarkupCommand(int MarkupGroupId) : ICommand;

public class SelectDefaultMarkupHandler(
    IDefaultSettingsRepository defaultSettingsRepository,
    IMessageBroker messageBroker,
    IMarkupRepository markupRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SelectDefaultMarkupCommand>
{
    public async Task<Unit> Handle(SelectDefaultMarkupCommand request, CancellationToken cancellationToken)
    {
        await defaultSettingsRepository.CreateDefaultSettingsIfNotExist(cancellationToken);
        var selectedMarkup = await defaultSettingsRepository.GetSettingForUpdateAsync("SelectedMarkupId",
            true, cancellationToken) ?? throw new SelectedMarkupNotFoundException();
        if (selectedMarkup.Value == request.MarkupGroupId.ToString()) return Unit.Value;

        if (request.MarkupGroupId == -1)
        {
            selectedMarkup.Value = "-1";
        }
        else
        {
            var markupGroup = await markupRepository.GetMarkupByIdAsync(request.MarkupGroupId, true, cancellationToken)
                              ?? throw new MarkupGroupNotFoundException(request.MarkupGroupId);
            selectedMarkup.Value = markupGroup.Id.ToString();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await messageBroker.Publish(new MarkupGroupChangedEvent(request.MarkupGroupId), cancellationToken);
        return Unit.Value;
    }
}