using Bogus;
using Main.Entities.Storage;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Storage;

public class StorageContentReservationBuilder(Faker faker) : BuilderBase<StorageContentReservation>(faker)
{
    public Guid? UserId { get; private set; }
    public int? ProductId { get; private set; }
    public int? ReservedCount { get; private set; }
    public int? CurrentCount { get; private set; }
    public decimal? ProposedPrice { get; private set; }
    public int? ProposedCurrencyId { get; private set; }
    public string? Comment { get; private set; }

    public StorageContentReservationBuilder WithUserId(Guid userId)
    {
        UserId = userId;
        return this;
    }

    public StorageContentReservationBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public StorageContentReservationBuilder WithReservedCount(int reservedCount)
    {
        ReservedCount = reservedCount;
        return this;
    }

    public StorageContentReservationBuilder WithCurrentCount(int currentCount)
    {
        CurrentCount = currentCount;
        return this;
    }

    public StorageContentReservationBuilder WithProposedPrice(decimal? price, int? currencyId)
    {
        ProposedPrice = price;
        ProposedCurrencyId = currencyId;
        return this;
    }

    public StorageContentReservationBuilder WithComment(string? comment)
    {
        Comment = comment;
        return this;
    }

    public override StorageContentReservation Build()
    {
        var reservation = StorageContentReservation.Create(
            UserId ?? Guid.NewGuid(),
            ProductId ?? Faker.Random.Int(1),
            ReservedCount ?? Faker.Random.Int(1, 100));

        reservation.ProposePrice(ProposedPrice, ProposedCurrencyId);
        reservation.SetComment(Comment);

        if (CurrentCount is not null)
            reservation.AddCount(CurrentCount.Value);

        return reservation;
    }
}
