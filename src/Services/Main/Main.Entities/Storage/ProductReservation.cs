using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Exceptions;
using Main.Enums;

namespace Main.Entities.Storage;

public class ProductReservation : AuditableEntity<ProductReservation, int>,
    ILinqEntity<ProductReservation, int>
{
    private ProductReservation() { }

    private ProductReservation(
        Guid organizationId,
        int productId,
        int reservedCount)
    {
        OrganizationId = organizationId;
        ProductId = productId;
        CurrentCount = 0;
        Status = ProductReservationStatus.Active;
        SetReservedCount(reservedCount);
    }

    public int Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public int ProductId { get; private set; }

    public int ReservedCount { get; private set; }

    public int CurrentCount { get; private set; }

    public decimal? ProposedPrice { get; private set; }

    public int? ProposedCurrencyId { get; private set; }

    public ProductReservationStatus Status { get; private set; }

    public string? Comment { get; private set; }

    public Organization.Organization Organization { get; private set; } = null!;

    public static Expression<Func<ProductReservation, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<ProductReservation, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static ProductReservation Create(
        Guid organizationId,
        int productId,
        int reservedCount)
    {
        return new ProductReservation(
            organizationId,
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

    public void Cancel() { Status = ProductReservationStatus.Canceled; }

    private void UpdateStatus()
    {
        if (Status == ProductReservationStatus.Canceled) return;

        Status = CurrentCount switch
        {
            _ when CurrentCount == ReservedCount => ProductReservationStatus.Done,
            > 0 => ProductReservationStatus.Locked,
            _ => ProductReservationStatus.Active
        };
    }

    private void PerformDomainChecks()
    {
        ThrowIfDone();
        ThrowIfCanceled();
    }

    private void ThrowIfDone()
    {
        if (Status == ProductReservationStatus.Done)
            throw new InvalidInputException("article.reservation.is.done");
    }

    private void ThrowIfCanceled()
    {
        if (Status == ProductReservationStatus.Canceled)
            throw new InvalidInputException("article.reservation.is.canceled");
    }

    public override int GetId() { return Id; }
}
