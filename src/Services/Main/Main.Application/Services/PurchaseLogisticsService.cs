using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Main.Application.Dtos.Logistics;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;

namespace Main.Application.Services;

public class PurchaseLogisticsService(
    ISender sender,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IPurchaseLogisticsService
{
    public async Task ApplyAsync(
        Purchase purchase,
        IEnumerable<PurchaseLogisticsItem> items,
        string? storageFrom,
        DateTime purchaseDateTime,
        Guid systemUserId,
        CancellationToken cancellationToken = default)
    {
        var selectedItems = items.ToList();
        if (selectedItems.Count == 0)
        {
            ClearLogistics(purchase);
            return;
        }

        if (storageFrom == null)
            throw new InvalidOperationException("Storage from must be set, when calculation logistics.");

        var deliveryCost = await sender.Send(
            new CalculateDeliveryCostQuery(
                storageFrom,
                purchase.Storage,
                selectedItems.Select(x => new LogisticsItemDto
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }),
                LogisticsCalculationMode.Strict),
            cancellationToken);

        var route = deliveryCost.Route;
        for (var i = 0; i < selectedItems.Count; i++)
        {
            var calcResult = deliveryCost.DeliveryCost.Items[i];
            selectedItems[i].PurchaseContent.SetLogistic(
                calcResult.Weight,
                calcResult.AreaM3,
                calcResult.Cost);
        }

        foreach (var content in purchase.Contents.Except(selectedItems.Select(x => x.PurchaseContent)).ToList())
            if (content.ClearLogistic() is { } logistic)
                unitOfWork.Remove(logistic);

        Transaction? logisticsPayment = null;
        if (route.CarrierId is not null)
        {
            var carrier = await userRepository.EnsureExistAsync(
                route.CarrierId.Value,
                id => new UserNotFoundException(id),
                ct: cancellationToken);

            logisticsPayment = (await sender.Send(
                    new CreateTransactionCommand(
                        carrier.Id,
                        systemUserId,
                        deliveryCost.DeliveryCost.TotalCost,
                        route.Currency.Id,
                        purchaseDateTime,
                        TransactionSourceType.Logistic,
                        TransactionCreationMode.System),
                    cancellationToken))
                .Transaction;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        purchase.SetPurchaseLogistic(
            route.Id,
            route.Currency.Id,
            route.PricingModel,
            route.RouteType,
            route.PricePerKg,
            route.PricePerM3,
            route.PricePerOrder,
            route.MinimumPrice,
            logisticsPayment?.Id,
            deliveryCost.DeliveryCost.MinimalPriceApplied);
    }

    private void ClearLogistics(Purchase purchase)
    {
        foreach (var content in purchase.Contents)
            if (content.ClearLogistic() is { } logistic)
                unitOfWork.Remove(logistic);

        if (purchase.ClearPurchaseLogistic() is { } purchaseLogistic)
            unitOfWork.Remove(purchaseLogistic);
    }
}
