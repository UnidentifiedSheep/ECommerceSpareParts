using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Exceptions;
using Main.Enums;

namespace Main.Entities.Storage;

//Product reservation. It should be renamed.
public class StorageContentReservation : AuditableEntity<StorageContentReservation, int>,
    ILinqEntity<StorageContentReservation, int>
{
    private StorageContentReservation() { }

    private StorageContentReservation(
        Guid userId,
        int productId,
        int reservedCount)
    {
        UserId = userId;
        ProductId = productId;
        CurrentCount = 0;
        Status = StorageContentReservationStatus.Active;
        SetReservedCount(reservedCount);
    }

    public int Id { get; private set; }

    public Guid UserId { get; private set; }

    public int ProductId { get; private set; }

    public int ReservedCount { get; private set; }

    public int CurrentCount { get; private set; }

    public decimal? ProposedPrice { get; private set; }

    public int? ProposedCurrencyId { get; private set; }

    public StorageContentReservationStatus Status { get; private set; }

    public string? Comment { get; private set; }

    public User.User User { get; private set; } = null!;

    public static Expression<Func<StorageContentReservation, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<StorageContentReservation, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static StorageContentReservation Create(
        Guid userId,
        int productId,
        int reservedCount)
    {
        return new StorageContentReservation(
            userId,
            productId,
            reservedCount);
    }

    private void SetReservedCount(int initialCount)
    {
        initialCount.EnsureAtLeast(1, "article.reservation.initial.count.must.be.positive");
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
            .EnsureMaxDecimalPlaces(2, "article.reservation.proposed.price.max.two.decimals")
            .EnsureAtLeast(0, "article.reservation.given.price.must.be.positive");

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

        comment.EnsureMaxLength(500, "article.reservation.comment.max.length");
        Comment = comment;
    }

    public void AddCount(int amount)
    {
        ThrowIfCanceled();

        var summed = CurrentCount + amount;
        if (summed > ReservedCount) throw new InvalidOperationException("Can't increase reservation count");
        if (summed < 0) throw new InvalidOperationException("Can't decrease reservation count");

        CurrentCount = summed;

        UpdateStatus();
    }

    public void Cancel() { Status = StorageContentReservationStatus.Canceled; }

    private void UpdateStatus()
    {
        if (Status == StorageContentReservationStatus.Canceled) return;

        Status = CurrentCount switch
        {
            _ when CurrentCount == ReservedCount => StorageContentReservationStatus.Done,
            > 0 => StorageContentReservationStatus.Locked,
            _ => StorageContentReservationStatus.Active
        };
    }

    private void PerformDomainChecks()
    {
        ThrowIfDone();
        ThrowIfCanceled();
    }

    private void ThrowIfDone()
    {
        if (Status == StorageContentReservationStatus.Done)
            throw new InvalidInputException("article.reservation.is.done");
    }

    private void ThrowIfCanceled()
    {
        if (Status == StorageContentReservationStatus.Canceled)
            throw new InvalidInputException("article.reservation.is.canceled");
    }

    public override int GetId() { return Id; }
}