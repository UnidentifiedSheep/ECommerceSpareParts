using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Purchase;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;

[Transactional]
public record UpsertPurchaseLogisticsCommand(
    Guid PurchaseId,
    Guid RouteId,
    Guid? TransactionId,
    bool MinimumPriceApplied) : ICommand;

public class UpsertPurchaseLogisticsHandler(
    IUnitOfWork unitOfWork) : ICommandHandler<UpsertPurchaseLogisticsCommand>
{
    public async Task<Unit> Handle(UpsertPurchaseLogisticsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}