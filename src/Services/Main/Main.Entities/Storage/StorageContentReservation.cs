using Domain;
using Domain.Extensions;
using Exceptions;

namespace Main.Entities.Storage;

public class StorageContentReservation : AuditableEntity<StorageContentReservation, int>
{
    public int Id { get; private set; }

    public Guid UserId { get; private set; }

    public int ProductId { get; private set; }

    public int ReservedCount { get; private set; }

    public int CurrentCount { get; private set; }

    public decimal? ProposedPrice { get; private set; }

    public int? ProposedCurrencyId { get; private set; }

    public bool IsDone { get; private set; }
    
    public bool IsLocked { get; private set; }

    public string? Comment { get; private set; }
    
    private StorageContentReservation() {}

    private StorageContentReservation(
        Guid userId,
        int productId,
        int reservedCount)
    {
        UserId = userId;
        ProductId = productId;
        CurrentCount = 0;
        SetReservedCount(reservedCount);
    }

    public static StorageContentReservation Create(
        Guid userId,
        int productId,
        int reservedCount)
    {
        return new StorageContentReservation(userId, productId, reservedCount);
    }

    private void SetReservedCount(int initialCount)
    {
        initialCount.AgainstTooSmall(1, "article.reservation.initial.count.must.be.positive");
        ReservedCount = initialCount;
    }

    public void ProposePrice(decimal? givenPrice, int? givenCurrencyId)
    {
        PerformDomainChecks();
        
        var hasPrice = givenPrice.HasValue;
        var hasCurrency = givenCurrencyId.HasValue;

        if (hasPrice != hasCurrency)
            throw new InvalidInputException("article.reservation.given.price.with.out.currency");
        
        if (givenPrice == null)
        {
            ProposedPrice = null;
            ProposedCurrencyId = null;
            return;
        }

        givenPrice.Value
            .AgainstTooManyDecimalPlaces(2, "article.reservation.proposed.price.max.two.decimals")
            .AgainstTooSmall(0, "article.reservation.given.price.must.be.positive");
        
        ProposedPrice = givenPrice;
        ProposedCurrencyId = givenCurrencyId;
    }

    public void SetComment(string? comment)
    {
        comment = comment?.Trim();
        if (string.IsNullOrEmpty(comment))
        {
            Comment = null;
            return;
        }

        comment.AgainstTooLong(500, "article.reservation.comment.max.length");
        Comment = comment;
    }

    public void AddCount(int amount)
    {
        var summed = CurrentCount + amount;
        if (summed > ReservedCount)
            throw new InvalidOperationException("Can't increase reservation count");
        if (summed < 0)
            throw new InvalidOperationException("Can't decrease reservation count");
        
        CurrentCount = summed;

        if (CurrentCount == ReservedCount)
            MarkAsDone();
        else
            MarkAdNotDone();
    }

    public void Lock()
    {
        ThrowIfDone();
        IsLocked = true;
    }

    public void Unlock()
    {
        ThrowIfDone();
        IsLocked = false;
    }

    private void MarkAsDone()
    {
        IsDone = true;
    }

    private void MarkAdNotDone()
    {
        IsDone = false;
    }

    private void PerformDomainChecks()
    {
        ThrowIfLocked();
        ThrowIfDone();
    }

    private void ThrowIfLocked()
    {
        if (IsLocked)
            throw new InvalidInputException("article.reservation.is.locked");
    }

    private void ThrowIfDone()
    {
        if (IsDone)
            throw new InvalidInputException("article.reservation.is.done");
    }
    
    public override int GetId() => Id;
}