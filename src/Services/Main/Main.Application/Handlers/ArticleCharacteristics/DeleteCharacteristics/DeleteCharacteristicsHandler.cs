using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleCharacteristics;
using Main.Core.Interfaces.DbRepositories;
using MediatR;

namespace Main.Application.Handlers.ArticleCharacteristics.DeleteCharacteristics;

[Transactional]
[ExceptionType<ArticleCharacteristicsNotFoundException>]
public record DeleteCharacteristicsCommand(int Id) : ICommand;

public class DeleteCharacteristicsHandler(
    IArticleCharacteristicsRepository repository,
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