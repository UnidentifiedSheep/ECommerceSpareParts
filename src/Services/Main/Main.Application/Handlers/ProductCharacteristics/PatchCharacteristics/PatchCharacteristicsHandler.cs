using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Dtos.Amw.ArticleCharacteristics;
using MediatR;

namespace Main.Application.Handlers.ProductCharacteristics.PatchCharacteristics;

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