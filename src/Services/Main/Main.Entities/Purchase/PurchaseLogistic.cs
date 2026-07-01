using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Entities.Purchase;

public class PurchaseLogistic : Entity<PurchaseLogistic, Guid>, ILinqEntity<PurchaseLogistic, Guid>
{
    private PurchaseLogistic() { }

    private PurchaseLogistic(
        Guid purchaseId,
        Guid routeId,
        int currencyId,
        Guid? transactionId,
        LogisticPricingType pricingModel,
        RouteType routeType,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal? minimumPrice,
        bool minimumPriceApplied)
    {
        PurchaseId = purchaseId;
        SetRouteId(routeId);
        SetCurrencyId(currencyId);
        SetTransactionId(transactionId);
        SetPricingModel(pricingModel);
        SetRouteType(routeType);
        SetPrices(
            priceKg,
            pricePerM3,
            pricePerOrder);
        SetMinimumPrice(minimumPrice);
        SetMinimumPriceApplied(minimumPriceApplied);
    }

    [Validate]
    public Guid PurchaseId { get; }

    public Guid RouteId { get; private set; }

    public int CurrencyId { get; private set; }

    public Guid? TransactionId { get; private set; }

    public LogisticPricingType PricingModel { get; private set; }

    public RouteType RouteType { get; private set; }

    public decimal PriceKg { get; private set; }

    public decimal PricePerM3 { get; private set; }

    public decimal PricePerOrder { get; private set; }

    public decimal? MinimumPrice { get; private set; }

    public bool MinimumPriceApplied { get; private set; }

    public virtual Currency.Currency Currency { get; private set; } = null!;

    public virtual Transaction? Transaction { get; private set; }

    public static Expression<Func<PurchaseLogistic, Guid>> GetKeySelector() { return x => x.PurchaseId; }

    public static Expression<Func<PurchaseLogistic, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.PurchaseId == key;
    }

    internal static PurchaseLogistic Create(
        Guid purchaseId,
        Guid routeId,
        int currencyId,
        Guid? transactionId,
        LogisticPricingType pricingModel,
        RouteType routeType,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal? minimumPrice,
        bool minimumPriceApplied)
    {
        return new PurchaseLogistic(
            purchaseId,
            routeId,
            currencyId,
            transactionId,
            pricingModel,
            routeType,
            priceKg,
            pricePerM3,
            pricePerOrder,
            minimumPrice,
            minimumPriceApplied);
    }

    public void Update(
        Guid routeId,
        int currencyId,
        Guid? transactionId,
        LogisticPricingType pricingModel,
        RouteType routeType,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal? minimumPrice,
        bool minimumPriceApplied)
    {
        SetRouteId(routeId);
        SetCurrencyId(currencyId);
        SetTransactionId(transactionId);
        SetPricingModel(pricingModel);
        SetRouteType(routeType);
        SetPrices(
            priceKg,
            pricePerM3,
            pricePerOrder);
        SetMinimumPrice(minimumPrice);
        SetMinimumPriceApplied(minimumPriceApplied);
    }

    public void UpdateTariffs(
        int currencyId,
        LogisticPricingType pricingModel,
        RouteType routeType,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal? minimumPrice)
    {
        SetCurrencyId(currencyId);
        SetPricingModel(pricingModel);
        SetRouteType(routeType);
        SetPrices(
            priceKg,
            pricePerM3,
            pricePerOrder);
        SetMinimumPrice(minimumPrice);
    }

    public void AssignTransaction(Guid transactionId) { SetTransactionId(transactionId); }

    public void ClearTransaction() { SetTransactionId(null); }

    public void ApplyMinimumPrice() { SetMinimumPriceApplied(true); }

    public void CancelMinimumPrice() { SetMinimumPriceApplied(false); }

    private void SetRouteId(Guid routeId) { RouteId = routeId; }

    private void SetCurrencyId(int currencyId) { CurrencyId = currencyId; }

    private void SetTransactionId(Guid? transactionId) { TransactionId = transactionId; }

    private void SetPricingModel(LogisticPricingType pricingModel) { PricingModel = pricingModel; }

    private void SetRouteType(RouteType routeType) { RouteType = routeType; }

    private void SetPrices(
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder)
    {
        SetPriceKg(priceKg);
        SetPricePerM3(pricePerM3);
        SetPricePerOrder(pricePerOrder);
    }

    private void SetPriceKg(decimal priceKg)
    {
        PriceKg = priceKg.AgainstTooManyDecimalPlaces(2, "storage.route.price.kg.precision")
            .AgainstTooSmall(0, "storage.route.price.kg.min");
    }

    private void SetPricePerM3(decimal pricePerM3)
    {
        PricePerM3 = pricePerM3.AgainstTooManyDecimalPlaces(2, "storage.route.price.m3.precision")
            .AgainstTooSmall(0, "storage.route.price.m3.min");
    }

    private void SetPricePerOrder(decimal pricePerOrder)
    {
        PricePerOrder = pricePerOrder.AgainstTooManyDecimalPlaces(2, "storage.route.price.order.precision")
            .AgainstTooSmall(0, "storage.route.price.order.min");
    }

    private void SetMinimumPrice(decimal? minimumPrice)
    {
        if (minimumPrice is null)
        {
            MinimumPrice = null;
            return;
        }

        MinimumPrice = minimumPrice.Value
            .AgainstTooManyDecimalPlaces(2, "storage.route.minimum.price.precision")
            .AgainstTooSmall(0, "storage.route.minimum.price.min");
    }

    private void SetMinimumPriceApplied(bool minimumPriceApplied)
    {
        MinimumPriceApplied = minimumPriceApplied;
    }

    public override Guid GetId() { return PurchaseId; }
}