using Bogus;
using Main.Entities.Product;
using Main.Entities.Storage;
using Tests.Abstractions;

namespace Tests.DataBuilders.Storage;

public class ProductReservationBuilder(Faker faker) : BuilderBase<ProductReservation>(faker)
{
    private readonly HashSet<int> _productIds = [];
    private readonly HashSet<Guid> _organizationIds = [];

    public Guid? OrganizationId { get; private set; }
    public int? ProductId { get; private set; }
    public int? ReservedCount { get; private set; }
    public int? CurrentCount { get; private set; }
    public decimal? ProposedPrice { get; private set; }
    public int? ProposedCurrencyId { get; private set; }
    public string? Comment { get; private set; }

    public ProductReservationBuilder WithOrganizationId(Guid organizationId)
    {
        OrganizationId = organizationId;
        return this;
    }

    public ProductReservationBuilder WithOrganizationIds(params Guid[] organizationIds)
    {
        _organizationIds.UnionWith(organizationIds);
        return this;
    }

    public ProductReservationBuilder WithProductId(int productId)
    {
        ProductId = productId;
        return this;
    }

    public ProductReservationBuilder WithProductIds(params int[] productIds)
    {
        _productIds.UnionWith(productIds);
        return this;
    }

    public ProductReservationBuilder WithProducts(IEnumerable<Product> products)
    {
        _productIds.UnionWith(products.Select(x => x.Id));
        return this;
    }

    public ProductReservationBuilder WithReservedCount(int reservedCount)
    {
        ReservedCount = reservedCount;
        return this;
    }

    public ProductReservationBuilder WithCurrentCount(int currentCount)
    {
        CurrentCount = currentCount;
        return this;
    }

    public ProductReservationBuilder WithProposedPrice(decimal? price, int? currencyId)
    {
        ProposedPrice = price;
        ProposedCurrencyId = currencyId;
        return this;
    }

    public ProductReservationBuilder WithComment(string? comment)
    {
        Comment = comment;
        return this;
    }

    public override ProductReservation Build()
    {
        var reservation = ProductReservation.Create(
            OrganizationId ??
            (_organizationIds.Count > 0
                ? Faker.PickRandom<Guid>(_organizationIds)
                : Guid.NewGuid()),
            ProductId ?? (_productIds.Count > 0 ? Faker.PickRandom<int>(_productIds) : Faker.Random.Int(1)),
            ReservedCount ?? Faker.Random.Int(1, 100));

        reservation.ProposePrice(ProposedPrice, ProposedCurrencyId);
        reservation.SetComment(Comment);

        if (CurrentCount is not null) reservation.AddCount(CurrentCount.Value);

        return reservation;
    }
}
