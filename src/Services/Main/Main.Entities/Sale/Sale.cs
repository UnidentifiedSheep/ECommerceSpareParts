using Domain;
using Domain.Extensions;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Entities.Sale;

public class Sale : AuditableEntity<Sale, Guid>
{
    public Guid Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public Guid TransactionId { get; private set; }
    public int CurrencyId { get; private set; }
    public string StorageName { get; private set; } = null!;
    public string? Comment { get; private set; }
    public DateTime SaleDatetime { get; private set; }
    public SaleState State { get; private set; }
    public User.User Buyer { get; private set; } = null!;
    public Currency.Currency Currency { get; private set; } = null!;
    public Transaction Transaction { get; private set; } = null!;
    private readonly List<SaleContent> _contents = [];
    public IReadOnlyList<SaleContent> Contents => _contents;
    
    private Sale() {}

    private Sale(Guid buyerId, Guid transactionId, int currencyId, string storageName, DateTime saleDate)
    {
        TransactionId = transactionId;
        CurrencyId = currencyId;
        BuyerId = buyerId;
        StorageName = storageName;
        SaleDatetime = saleDate;
        State = SaleState.Draft;
    }

    public static Sale Create(Guid buyerId, Guid transactionId, int currencyId, string storageName, DateTime saleDate)
    {
        return new Sale(buyerId, transactionId, currencyId, storageName, saleDate);
    }

    public void SetComment(string? comment)
    {
        Comment = comment.NullIfWhiteSpace()?
            .AgainstTooLong(256, "sale.comment.max");
    }

    public void AddContent(SaleContent content)
    {
        _contents.Add(content);
    }

    public override Guid GetId() => Id;
}