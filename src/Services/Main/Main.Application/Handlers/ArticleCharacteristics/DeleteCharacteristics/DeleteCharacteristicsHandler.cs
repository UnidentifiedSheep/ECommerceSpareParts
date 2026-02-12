using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.ArticleCharacteristics;
using Main.Abstractions.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.ArticleCharacteristics.DeleteCharacteristics;

[Transactional]
public record DeleteCharacteristicsCommand(int Id) : ICommand;

public class DeleteCharacteristicsHandler(IArticleCharacteristicsRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCharacteristicsCommand>
{
    public async Task<Unit> Handle(DeleteCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetCharacteristic(request.Id, true, cancellationToken)
                     ?? throw new ArticleCharacteristicsNotFoundException(request.Id);
        unitOfWork.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}