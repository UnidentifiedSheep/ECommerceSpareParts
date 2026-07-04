using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductCharacteristics.AddCharacteristics;

[AutoSave]
[Transactional]
public record AddCharacteristicsCommand(IEnumerable<NewCharacteristicsDto> Characteristics)
    : ICommand;

public class AddCharacteristicsHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<AddCharacteristicsCommand>
{
    public async Task<Unit> Handle(
        AddCharacteristicsCommand request,
        CancellationToken cancellationToken)
    {
        var toAdd = new List<ProductCharacteristic>();
        foreach (var @new in request.Characteristics)
            toAdd.Add(
                ProductCharacteristic.Create(
                    @new.ProductId,
                    @new.Name,
                    @new.Value));

        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        return Unit.Value;
    }
}