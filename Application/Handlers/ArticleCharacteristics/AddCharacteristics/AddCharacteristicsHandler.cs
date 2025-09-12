using Application.Interfaces;
using Core.Attributes;
using Core.Dtos.Amw.ArticleCharacteristics;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Services;
using Mapster;
using MediatR;

namespace Application.Handlers.ArticleCharacteristics.AddCharacteristics;

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