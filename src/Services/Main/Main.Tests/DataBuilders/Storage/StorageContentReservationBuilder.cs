using Bogus;
using Main.Entities.Product;
using Main.Entities.Storage;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Storage;

public class StorageContentReservationBuilder(Faker faker) : BuilderBase<StorageContentReservation>(faker)
{
    private readonly HashSet<int> _productIds = [];
    private readonly HashSet<Guid> _userIds = [];

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

    public StorageContentReservationBuilder WithUserIds(params Guid[] userIds)
    {
        _userIds.UnionWith(userIds);
        return this;
    }

    public StorageContentReservationBuilder WithUsers(IEnumerable<Main.Entities.User.User> users)
    {
        _userIds.UnionWith(users.Select(x => x.Id));
        return this;
    }

    public StorageContentReservationBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public StorageContentReservationBuilder WithProductIds(params int[] productIds)
    {
        _productIds.UnionWith(productIds);
        return this;
    }

    public StorageContentReservationBuilder WithProducts(IEnumerable<Product> products)
    {
        _productIds.UnionWith(products.Select(x => x.Id));
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
            UserId ?? (_userIds.Count > 0 ? Faker.PickRandom<Guid>(_userIds) : Guid.NewGuid()),
            ProductId ?? (_productIds.Count > 0 ? Faker.PickRandom<int>(_productIds) : Faker.Random.Int(1)),
            ReservedCount ?? Faker.Random.Int(1, 100));

        reservation.ProposePrice(ProposedPrice, ProposedCurrencyId);
        reservation.SetComment(Comment);

        if (CurrentCount is not null) reservation.AddCount(CurrentCount.Value);

        return reservation;
    }
}