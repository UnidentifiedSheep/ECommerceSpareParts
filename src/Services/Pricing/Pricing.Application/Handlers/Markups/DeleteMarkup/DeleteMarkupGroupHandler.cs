using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Attributes;
using Exceptions.Exceptions.Markups;
using MediatR;
using Pricing.Abstractions.Interfaces.DbRepositories;

namespace Pricing.Application.Handlers.Markups.DeleteMarkup;

[Transactional]
public record DeleteMarkupGroupCommand(int Id) : ICommand;

public class DeleteMarkupGroupHandler(IUnitOfWork unitOfWork, IMarkupRepository markupRepository,
    ISettingsContainer settingsContainer) : ICommandHandler<DeleteMarkupGroupCommand>
{
    public async Task<Unit> Handle(DeleteMarkupGroupCommand request, CancellationToken cancellationToken)
    {
        var markup = await markupRepository.GetMarkupByIdAsync(request.Id, true, cancellationToken)
                     ?? throw new MarkupGroupNotFoundException(request.Id);
        var setting = settingsContainer.GetSetting(Abstractions.Constants.Settings.Pricing);
        if (setting.SelectedMarkupId == request.Id)
            throw new MarkupGroupCanNotBeDeletedException();
        unitOfWork.Remove(markup);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}