using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities.Purchase;
using MediatR;

namespace Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;

[Transactional, AutoSave]
public record AddContentLogisticsToPurchaseCommand(IEnumerable<PurchaseContentLogisticDto> Contents) : ICommand;

public class AddContentLogisticsToPurchaseHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<AddContentLogisticsToPurchaseCommand>
{
    public async Task<Unit> Handle(AddContentLogisticsToPurchaseCommand request, CancellationToken cancellationToken)
    {
        var contentLogistics = request
            .Contents
            .Select(x => PurchaseContentLogistic.Create(x.PurchaseContentId, x.WeightKg, x.AreaM3, x.Price));
        await unitOfWork.AddRangeAsync(contentLogistics, cancellationToken);
        return Unit.Value;
    }
}