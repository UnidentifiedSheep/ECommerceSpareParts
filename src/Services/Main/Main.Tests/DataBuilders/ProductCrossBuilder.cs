using Bogus;
using Main.Entities.Product;
using Test.Common.Abstractions;

namespace Tests.DataBuilders;

public class ProductCrossBuilder(Faker faker) : BuilderBase<ProductCross>(faker)
{
    private readonly HashSet<int> _productIds = [];
    private readonly HashSet<(int Left, int Right)> _usedPairs = [];

    public int? LeftProductId { get; private set; }
    public int? RightProductId { get; private set; }
    public IReadOnlySet<int> ProductIds => _productIds;

    public ProductCrossBuilder WithLeftProductId(int productId)
    {
        LeftProductId = productId;
        return this;
    }

    public ProductCrossBuilder WithRightProductId(int productId)
    {
        RightProductId = productId;
        return this;
    }

    public ProductCrossBuilder WithProductIds(params int[] productIds)
    {
        _productIds.UnionWith(productIds);
        return this;
    }

    public ProductCrossBuilder WithProducts(IEnumerable<Product> products)
    {
        _productIds.UnionWith(products.Select(x => x.Id));
        return this;
    }

    public override ProductCross Build()
    {
        var (left, right) = GetPair();
        var productCross = ProductCross.Create(left, right);
        _usedPairs.Add((productCross.LeftProductId, productCross.RightProductId));
        return productCross;
    }

    private (int Left, int Right) GetPair()
    {
        if (LeftProductId.HasValue && RightProductId.HasValue)
            return (LeftProductId.Value, RightProductId.Value);

        if (_productIds.Count < 2)
            throw new InvalidOperationException("At least two product ids are required to build product crosses.");

        var availablePairs = GetAvailablePairs().ToList();
        if (availablePairs.Count == 0)
            throw new InvalidOperationException("No unique product crosses left to build.");

        return Faker.PickRandom(availablePairs);
    }

    private IEnumerable<(int Left, int Right)> GetAvailablePairs()
    {
        var productIds = _productIds.ToArray();

        for (var i = 0; i < productIds.Length - 1; i++)
        for (var j = i + 1; j < productIds.Length; j++)
        {
            var pair = (productIds[i], productIds[j]);
            if (!_usedPairs.Contains(pair))
                yield return pair;
        }
    }
}