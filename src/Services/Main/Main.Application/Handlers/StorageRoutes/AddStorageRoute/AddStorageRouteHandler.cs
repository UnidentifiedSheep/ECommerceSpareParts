using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Enums;

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
/// <param name="Status">Current route status. Active or not</param>
[Transactional]
public record AddStorageRouteCommand(string StorageFrom, string StorageTo, int Distance, RouteType RouteType, 
    LogisticPricingType PricingType, int DeliveryTime, decimal PriceKg, decimal PriceM3, 
    decimal PricePerOrder, RouteStatus Status) : ICommand<AddStorageRouteResult>;

public record AddStorageRouteResult(Guid RouteId);

public class AddStorageRouteHandler(IStorageRoutesRepository storageRoutesRepository, IUnitOfWork unitOfWork) 
    : ICommandHandler<AddStorageRouteCommand, AddStorageRouteResult>
{
    public async Task<AddStorageRouteResult> Handle(AddStorageRouteCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task ValidateData(string storageFrom, string storageTo, CancellationToken cancellationToken)
    {
    }
}