using Application.Interfaces;
using Core.Attributes;
using Core.Exceptions.Markups;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Interfaces.Services;
using MediatR;

namespace Application.Handlers.Markups.DeleteMarkup;

[Transactional]
public record DeleteMarkupGroupCommand(int Id) : ICommand;
public class DeleteMarkupGroupHandler(IUnitOfWork unitOfWork, IMarkupRepository markupRepository,
    IDefaultSettingsRepository defaultSettingsRepository) : ICommandHandler<DeleteMarkupGroupCommand>
{
    public async Task<Unit> Handle(DeleteMarkupGroupCommand request, CancellationToken cancellationToken)
    {
        var markup = await markupRepository.GetMarkupByIdAsync(request.Id, true, cancellationToken)
                     ?? throw new MarkupGroupNotFoundException(request.Id);
        var defaultSettings = await defaultSettingsRepository.GetDefaultSettingsAsync(cancellationToken);
        if(defaultSettings.SelectedMarkupId == request.Id)
            throw new MarkupGroupCanNotBeDeletedException();
        unitOfWork.Remove(markup);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}