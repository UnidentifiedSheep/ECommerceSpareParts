using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Entities.Balance;
using Main.Enums;

namespace Main.Entities.Sale;

public class Sale : AuditableEntity<Sale, Guid>, ILinqEntity<Sale, Guid>, IVersionable<uint>
{
    private readonly List<SaleContent> _contents = [];

    private Sale() { }

    private Sale(
        Guid buyerId,
        Guid transactionId,
        int currencyId,
        string storageName,
        DateTime saleDate)
    {
        TransactionId = transactionId;
        CurrencyId = currencyId;
        BuyerId = buyerId;
        StorageName = storageName;
        SaleDatetime = saleDate;
        State = SaleState.Draft;
    }

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
    public IReadOnlyList<SaleContent> Contents => _contents;

    public static Expression<Func<Sale, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<Sale, bool>> GetEqualityExpression(Guid key) { return x => x.Id == key; }

    public uint RowVersion { get; private set; }

    public static Sale Create(
        Guid buyerId,
        Guid transactionId,
        int currencyId,
        string storageName,
        DateTime saleDate)
    {
        return new Sale(
            buyerId,
            transactionId,
            currencyId,
            storageName,
            saleDate);
    }

    public void SetComment(string? comment)
    {
        Comment = comment.NullIfWhiteSpace()
            ?
            .EnsureMaxLength(256, "sale.comment.max");
    }

    public void AddContent(SaleContent content)
    {
        if (content.SaleId != Guid.Empty && content.SaleId != Id)
            throw new InvalidOperationException("Content already added to another sale");
        _contents.Add(content);
    }

    public void RemoveContent(SaleContent content)
    {
        if (content.SaleId != GetId()) throw new InvalidOperationException("Invalid sale id in sale content");
        _contents.Remove(content);
    }

    public void SetDateTime(DateTime dateTime) { SaleDatetime = dateTime; }

    public void SetCurrency(int currencyId) { CurrencyId = currencyId; }

    public void SetTransactionId(Guid transactionId) { TransactionId = transactionId; }

    public void Complete()
    {
        if (State == SaleState.Deleted) throw new InvalidOperationException("Cannot complete deleted sale");

        if (Contents.Count == 0) throw new InvalidOperationException("Cannot complete empty sale");

        State = SaleState.Completed;
    }

    public void Delete()
    {
        if (State == SaleState.Deleted) return;

        State = SaleState.Deleted;
    }

    public override Guid GetId() { return Id; }
}