using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Interfaces;
using Domain.Extensions;
using Enums;
using Exceptions;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Entities.Purchase;

public class Purchase : AuditableEntity<Purchase, Guid>, ILinqEntity<Purchase, Guid>
{
    private readonly List<PurchaseContent> _contents = [];

    private Purchase()
    {
    }

    private Purchase(
        Guid supplierId,
        int currencyId,
        Guid transactionId,
        string storage,
        DateTime purchaseDatetime)
    {
        SupplierId = supplierId;
        SetPurchaseDate(purchaseDatetime);
        SetCurrencyId(currencyId);
        TransactionId = transactionId;
        Storage = storage;
        State = PurchaseState.Draft;
    }

    [Validate]
    public Guid Id { get; private set; }

    public Guid SupplierId { get; private set; }
    public int CurrencyId { get; private set; }
    public Guid TransactionId { get; private set; }
    public string Storage { get; private set; } = null!;
    public DateTime PurchaseDatetime { get; private set; }
    public string? Comment { get; private set; }
    public PurchaseState State { get; private set; }
    public virtual Currency.Currency Currency { get; private set; } = null!;
    public virtual PurchaseLogistic? PurchaseLogistic { get; private set; }
    public virtual User.User Supplier { get; private set; } = null!;
    public virtual Transaction Transaction { get; private set; } = null!;
    public IReadOnlyCollection<PurchaseContent> Contents => _contents;

    public static Purchase Create(
        Guid supplierId,
        int currencyId,
        Guid transactionId,
        string storage,
        DateTime purchaseDatetime)
    {
        return new Purchase(supplierId, currencyId, transactionId, storage, purchaseDatetime);
    }

    public void SetComment(string? comment)
    {
        Comment = comment
            .NullIfWhiteSpace()?
            .AgainstTooLong(
                256,
                () => throw new InvalidInputException("purchase.comment.too.long"));
    }

    public void SetCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
    }

    public void SetPurchaseDate(DateTime purchaseDatetime)
    {
        PurchaseDatetime = purchaseDatetime;
    }

    public void AddContent(PurchaseContent content)
    {
        if (content.PurchaseId != GetId())
            throw new InvalidOperationException("Invalid purchase id in purchase content");
        if (_contents.Contains(content)) return;
        _contents.Add(content);
    }

    public void SetPurchaseLogistic(
        Guid routeId,
        int logisticsCurrencyId,
        LogisticPricingType pricingModel,
        RouteType routeType,
        decimal priceKg,
        decimal pricePerM3,
        decimal pricePerOrder,
        decimal? minimumPrice,
        Guid? transactionId,
        bool minimumPriceApplied)
    {
        if (PurchaseLogistic == null)
            PurchaseLogistic = PurchaseLogistic.Create(
                GetId(),
                routeId,
                logisticsCurrencyId,
                transactionId,
                pricingModel,
                routeType,
                priceKg,
                pricePerM3,
                pricePerOrder,
                minimumPrice,
                minimumPriceApplied);
        else
            PurchaseLogistic.Update(
                routeId,
                logisticsCurrencyId,
                transactionId,
                pricingModel,
                routeType,
                priceKg,
                pricePerM3,
                pricePerOrder,
                minimumPrice,
                minimumPriceApplied);
    }

    public void Complete()
    {
        State = PurchaseState.Completed;
    }

    public override Guid GetId()
    {
        return Id;
    }

    public static Expression<Func<Purchase, bool>> GetEqualityExpression(Guid key)
        => x => x.Id == key;
}
