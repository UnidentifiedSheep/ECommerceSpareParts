using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;

[Transactional]
public record AddContentLogisticsToPurchaseCommand(IEnumerable<PurchaseContentLogisticDto> Contents) : ICommand;

public class AddContentLogisticsToPurchaseHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddContentLogisticsToPurchaseCommand>
{
    public async Task<Unit> Handle(AddContentLogisticsToPurchaseCommand request, CancellationToken cancellationToken)
    {
        var models = request.Contents.Adapt<List<PurchaseContentLogistic>>();
        await unitOfWork.AddRangeAsync(models, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}