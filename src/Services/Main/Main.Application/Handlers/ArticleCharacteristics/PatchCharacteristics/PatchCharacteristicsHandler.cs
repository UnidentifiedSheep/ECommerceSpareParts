using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.ArticleCharacteristics;
using Main.Abstractions.Dtos.Amw.ArticleCharacteristics;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleCharacteristics.PatchCharacteristics;

[Transactional]
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