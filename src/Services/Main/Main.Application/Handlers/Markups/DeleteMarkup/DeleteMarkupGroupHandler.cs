using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Markups;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.Markups.DeleteMarkup;

[Transactional]
[ExceptionType<MarkupGroupNotFoundException>]
[ExceptionType<MarkupGroupCanNotBeDeletedException>]
public record DeleteMarkupGroupCommand(int Id) : ICommand;

public class DeleteMarkupGroupHandler(
    IUnitOfWork unitOfWork,
    IMarkupRepository markupRepository,
    IDefaultSettingsRepository defaultSettingsRepository) : ICommandHandler<DeleteMarkupGroupCommand>
{
    public async Task<Unit> Handle(DeleteMarkupGroupCommand request, CancellationToken cancellationToken)
    {
        var markup = await markupRepository.GetMarkupByIdAsync(request.Id, true, cancellationToken)
                     ?? throw new MarkupGroupNotFoundException(request.Id);
        var defaultSettings = await defaultSettingsRepository.GetDefaultSettingsAsync(cancellationToken);
        if (defaultSettings.SelectedMarkupId == request.Id)
            throw new MarkupGroupCanNotBeDeletedException();
        unitOfWork.Remove(markup);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}