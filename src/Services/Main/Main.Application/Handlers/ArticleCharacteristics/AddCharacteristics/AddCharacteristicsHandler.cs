using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Core.Dtos.Amw.ArticleCharacteristics;
using Main.Core.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.ArticleCharacteristics.AddCharacteristics;

[Transactional]
public record AddCharacteristicsCommand(IEnumerable<NewCharacteristicsDto> Characteristics) : ICommand;

public class AddCharacteristicsHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddCharacteristicsCommand>
{
    public async Task<Unit> Handle(AddCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var adapted = request.Characteristics.Adapt<List<ArticleCharacteristic>>();
        await unitOfWork.AddRangeAsync(adapted, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}