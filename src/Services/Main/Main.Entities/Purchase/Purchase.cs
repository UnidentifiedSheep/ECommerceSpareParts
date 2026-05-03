using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Enums;
using Exceptions;
using Main.Entities.Balance;

namespace Main.Entities.Purchase;

public class Purchase : AuditableEntity<Purchase, Guid>
{
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
    private readonly List<PurchaseContent> _contents = [];
    public IReadOnlyCollection<PurchaseContent> Contents => _contents;

    private Purchase() {}

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
                max: 256,
                exceptionFactory: () => throw new InvalidInputException("purchase.comment.too.long"));
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

    public void Complete()
    {
        State = PurchaseState.Completed;
    }
    
    public override Guid GetId() => Id;
}