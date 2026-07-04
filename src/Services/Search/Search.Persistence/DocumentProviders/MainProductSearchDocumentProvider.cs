using Extensions;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Product;
using Search.Application.Interfaces.Product;
using Search.Entities;

namespace Search.Persistence.DocumentProviders;

public class MainProductSearchDocumentProvider(
    IMainClient mainClient
) : IProductSearchDocumentProvider
{

    public async Task<Dictionary<int, Product?>> GetByIds(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idsList = ids.Distinct().ToList();
        var response = await mainClient.ProductNode.GetFullProduct(idsList, cancellationToken);

        if (!response.Success)
            throw new InvalidOperationException(
                $"Unable to get products from Main service. " +
                $"Status: {response.StatusCode}. " +
                $"Error: {response.Error}");

        var dict = response.ValueOrThrow
            .ToDictionary(x => x.Id);

        return idsList.ToDictionary(
            x => x,
            x => dict.TryGetValue(x, out var product)
                ? new Product
                {
                    Id = product.Id,
                    Sku = product.Sku,
                    NormalizedSku = product.Sku.OnlyCharacterToLower(),
                    Name = product.Name,
                    ProducerId = product.ProducerId,
                    Dimensions = MapDimensions(product.ProductSize),
                    Weight = MapWeight(product.ProductWeight),
                    Stock = product.Stock,
                    Indicator = product.Indicator
                }
                : null);
    }
    private static ProductDimensions? MapDimensions(InternalProductSize? size)
    {
        return size == null
            ? null
            : new ProductDimensions
            {
                Length = size.Length,
                LengthM = size.Length.ToMeters(size.Unit),
                Width = size.Width,
                WidthM = size.Width.ToMeters(size.Unit),
                Height = size.Height,
                HeightM = size.Height.ToMeters(size.Unit),
                Unit = size.Unit,
                VolumeM3 = size.VolumeM3
            };
    }

    private static ProductWeight? MapWeight(InternalProductWeight? weight)
    {
        return weight == null
            ? null
            : new ProductWeight
            {
                Value = weight.Weight,
                Unit = weight.Unit,
                WeightKg = weight.Weight.ToKg(weight.Unit)
            };
    }
}