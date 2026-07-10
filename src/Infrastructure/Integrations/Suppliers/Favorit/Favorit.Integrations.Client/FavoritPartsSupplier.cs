using System.Net;
using Enums;
using Favorit.Integrations.Core.Interfaces;
using Favorit.Integrations.Core.Models;
using Favorit.Integrations.Core.Requests;
using Favorit.Integrations.Core.Responses;
using Integrations.Common;
using Integrations.Supplier.Connections;
using Integrations.Supplier.Interfaces;
using Integrations.Supplier.Models;
using Integrations.Supplier.Models.Requests;
using Integrations.Supplier.Settings;

namespace Favorit.Integrations.Client;

public class FavoritPartsSupplier(
    IFavoritPartsClient client,
    ISupplierSettingsProvider<FavoriteSettings> settingsProvider,
    IConnectionProvider<FavoritConnection> connectionProvider
) : ISupplier
{
    public Supplier Supplier => Supplier.FavoritParts;

    public async Task<Response<IReadOnlyList<SupplierProduct>>> GetProductsAsync(
        GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await client.GetPricesAsync(
            AdaptRequest(request),
            cancellationToken);

        if (!result.Success)
            Response<IReadOnlyList<SupplierProduct>>.Fail(
                result.StatusCode ?? HttpStatusCode.InternalServerError,
                result.Error);

        var settings = await settingsProvider.GetSettingsAsync(cancellationToken);
        return Response<IReadOnlyList<SupplierProduct>>.Ok(
            AdaptResponse(result.ValueOrThrow, settings));
    }

    public async Task<ConnectionCheck> CheckConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await connectionProvider.CheckConnectionAsync(cancellationToken);
    }

    private static GetPricesRequest AdaptRequest(GetProductsRequest request)
    {
        return new GetPricesRequest
        {
            Brand = request.Brand,
            Number = request.Number,
            ShowAnalogues = request.ShowAnalogues,
            ShowIsRefundable = true
        };
    }

    private static List<SupplierProduct> AdaptResponse(
        GetPricesResponse response,
        FavoriteSettings settings)
    {
        return response.Goods
            .Select(good => AdaptGood(good, settings))
            .ToList();
    }

    private static SupplierProduct AdaptGood(Good good, FavoriteSettings settings)
    {
        return new SupplierProduct
        {
            Brand = good.Brand,
            Id = good.Id,
            Name = good.Name,
            Number = good.Number,
            Analogues = good.Analogues.Select(x => AdaptGood(x, settings)).ToList(),
            Positions = good.Warehouses.Select(x => new SupplierPosition
                {
                    Id = x.Id,
                    PurchaseInfo = new PurchaseInfo
                    {
                        AvailableQuantity = x.Stock,
                        DaysToRefund = 0,
                        PartnerWarehouse = !x.Own,
                        QuantityCoefficient = good.Rate,
                        MinimumPurchaseQuantity = 1,
                        PriceInfo = new PriceInfo { CurrencyCode = "RUB", Price = x.Price }
                    },
                    DeliveryInfo = new DeliveryInfo
                    {
                        DeliveryDate = x.ShipmentDate.UtcDateTime.Date.AddDays(settings.MinDaysToDelivery),
                        DeliveryProbability = 99,
                        GuaranteedDeliveryDate = x.ShipmentDate.Date.AddDays(settings.MaxDaysToDelivery),
                        OrderTill = DateTime.UtcNow.Date.Add(x.ShipmentDate.UtcDateTime.TimeOfDay)
                    }
                })
                .ToList()
        };
    }
}