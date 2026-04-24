using Application.Common.Extensions;
using Application.Common.Interfaces;
using Attributes;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Storages;
using MediatR;

namespace Main.Application.Handlers.StorageRoutes.EditStorageRoute;

[AutoSave]
[Transactional]
public record EditStorageRouteCommand(Guid Id, PatchStorageRouteDto PatchStorageRoute) : ICommand;

public class EditStorageRouteHandler(
    IStorageRouteRepository repository)
    : ICommandHandler<EditStorageRouteCommand>
{
    public async Task<Unit> Handle(EditStorageRouteCommand request, CancellationToken cancellationToken)
    {
        var storageRoute = await repository.GetById(request.Id, cancellationToken)
                           ?? throw new StorageRouteNotFound(request.Id);

        var patch = request.PatchStorageRoute;

        if (patch.IsActive.IsSet)
        {
            if (patch.IsActive.Value)
            {
                var isActiveExists = await repository.IsAnyRouteActiveAsync(
                    storageRoute.FromStorageName,
                    storageRoute.ToStorageName,
                    cancellationToken);
                
                if (isActiveExists)
                    throw new StorageRouteActiveExistsException(storageRoute.FromStorageName, storageRoute.ToStorageName);
                
                storageRoute.Activate();
            }
            else
                storageRoute.Deactivate();
        }

        patch.DistanceM.Apply(storageRoute.SetDistanceM);
        patch.RouteType.Apply(storageRoute.SetRouteType);
        patch.PricingModel.Apply(storageRoute.SetPricingModel);
        patch.DeliveryTimeMinutes.Apply(storageRoute.SetDeliveryTime);
        patch.PriceKg.Apply(storageRoute.SetPriceKg);
        patch.CurrencyId.Apply(storageRoute.SetCurrencyId);
        patch.PricePerM3.Apply(storageRoute.SetPricePerM3);
        patch.PricePerOrder.Apply(storageRoute.SetPricePerOrder);
        patch.MinimumPrice.Apply(x => storageRoute.SetMinimumPrice(x ?? 0));
        patch.CarrierId.Apply(storageRoute.SetCarrierId);

        return Unit.Value;
    }
}