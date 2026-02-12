using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Main.Entities;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.StorageRoutes.AddStorageRoute;

/// <summary>
/// Creates a new logistics route between two storages.
/// </summary>
/// <param name="StorageFrom">Source storage name.</param>
/// <param name="StorageTo">Destination storage name.</param>
/// <param name="Distance">Route distance in meters.</param>
/// <param name="RouteType">Type of route.</param>
/// <param name="PricingType">Pricing model used for delivery cost calculation.</param>
/// <param name="DeliveryTime">Estimated delivery time in minutes.</param>
/// <param name="PriceKg">Price per kilogram.</param>
/// <param name="PriceM3">Price per cubic meter.</param>
/// <param name="PricePerOrder">Fixed price per order (if applicable).</param>
[Transactional]
public record AddStorageRouteCommand(string StorageFrom, string StorageTo, int Distance, RouteType RouteType, 
    LogisticPricingType PricingType, int DeliveryTime, decimal PriceKg, decimal PriceM3, int CurrencyId,
    decimal PricePerOrder, decimal? MinimumPrice, Guid? CarrierId) : ICommand<AddStorageRouteResult>;

public record AddStorageRouteResult(Guid RouteId);

public class AddStorageRouteHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddStorageRouteCommand, AddStorageRouteResult>
{
    public async Task<AddStorageRouteResult> Handle(AddStorageRouteCommand request, CancellationToken cancellationToken)
    {
        var storageRoute = request.Adapt<StorageRoute>();
        await unitOfWork.AddAsync(storageRoute, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new AddStorageRouteResult(storageRoute.Id);
    }
}