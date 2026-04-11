using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Exceptions.Storages;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Purchase;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;

[Transactional]
public record UpsertPurchaseLogisticsCommand(
    string PurchaseId,
    Guid RouteId,
    Guid? TransactionId,
    bool MinimumPriceApplied) : ICommand;

public class UpsertPurchaseLogisticsHandler(
    IUnitOfWork unitOfWork,
    IStorageRoutesRepository storageRoutesRepository,
    IPurchaseLogisticsRepository purchaseLogisticsRepository) : ICommandHandler<UpsertPurchaseLogisticsCommand>
{
    public async Task<Unit> Handle(UpsertPurchaseLogisticsCommand request, CancellationToken cancellationToken)
    {
        var storageRoute = await storageRoutesRepository.GetStorageRouteAsync(request.RouteId, true, cancellationToken)
                           ?? throw new StorageRouteNotFound(request.RouteId);
        var options = new QueryOptions<PurchaseLogistic, string>()
        {
            Data = request.PurchaseId,
        }.WithTracking();
        var model = await purchaseLogisticsRepository.GetPurchaseLogistics(options, cancellationToken);

        if (model == null)
        {
            model = new PurchaseLogistic();
            await unitOfWork.AddAsync(model, cancellationToken);
        }

        storageRoute.Adapt(model);

        model.PurchaseId = request.PurchaseId;
        model.TransactionId = request.TransactionId;
        model.MinimumPriceApplied = request.MinimumPriceApplied;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}