using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.ArticleCharacteristics;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.ArticleCharacteristics.AddCharacteristics;

[Transactional]
public record AddCharacteristicsCommand(IEnumerable<NewCharacteristicsDto> Characteristics)
    : ICommand<AddCharacteristicsResult>;

public record AddCharacteristicsResult(IEnumerable<int> Ids);

public class AddCharacteristicsHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddCharacteristicsCommand, AddCharacteristicsResult>
{
    public async Task<AddCharacteristicsResult> Handle(AddCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var adapted = request.Characteristics.Adapt<List<ArticleCharacteristic>>();
        await unitOfWork.AddRangeAsync(adapted, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new (adapted.Select(x => x.Id));
    }
}