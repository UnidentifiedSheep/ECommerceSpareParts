using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using MediatR;

namespace Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;

[Transactional]
[AutoSave]
public record AddContentLogisticsToPurchaseCommand(IEnumerable<PurchaseContentLogisticDto> Contents) : ICommand;

public class AddContentLogisticsToPurchaseHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<AddContentLogisticsToPurchaseCommand>
{
    public async Task<Unit> Handle(AddContentLogisticsToPurchaseCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}