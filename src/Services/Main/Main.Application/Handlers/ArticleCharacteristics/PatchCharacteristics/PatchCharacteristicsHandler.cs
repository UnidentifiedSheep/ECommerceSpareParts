using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.ArticleCharacteristics;
using Main.Core.Dtos.Amw.ArticleCharacteristics;
using Main.Core.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleCharacteristics.PatchCharacteristics;

[Transactional]
[ExceptionType<ArticleCharacteristicsNotFoundException>]
public record PatchCharacteristicsCommand(int Id, PatchCharacteristicsDto NewValues) : ICommand;

public class PatchCharacteristicsHandler(
    IArticleCharacteristicsRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<PatchCharacteristicsCommand>
{
    public async Task<Unit> Handle(PatchCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetCharacteristic(request.Id, true, cancellationToken)
                     ?? throw new ArticleCharacteristicsNotFoundException(request.Id);
        request.NewValues.Adapt(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}