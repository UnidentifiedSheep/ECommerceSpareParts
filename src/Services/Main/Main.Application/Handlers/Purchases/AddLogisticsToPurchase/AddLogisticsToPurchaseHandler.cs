using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Purchases.AddLogisticsToPurchase;


[Transactional]
public record AddLogisticsToPurchaseCommand(string PurchaseId, Guid RouteId, Guid? TransactionId, 
    bool MinimumPriceApplied) : ICommand;

public class AddLogisticsToPurchaseHandler(IUnitOfWork unitOfWork, IStorageRoutesRepository storageRoutesRepository) 
    : ICommandHandler<AddLogisticsToPurchaseCommand>
{
    public async Task<Unit> Handle(AddLogisticsToPurchaseCommand request, CancellationToken cancellationToken)
    {
        var storageRoute = await storageRoutesRepository.GetStorageRouteAsync(request.RouteId, true, cancellationToken)
                           ?? throw new StorageRouteNotFound(request.RouteId);
        PurchaseLogistic model = storageRoute.Adapt<PurchaseLogistic>();

        model.PurchaseId = request.PurchaseId;
        model.TransactionId = request.TransactionId;
        model.MinimumPriceApplied = request.MinimumPriceApplied;
        
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}