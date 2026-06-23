using Extensions;
using Internal.Integration.Core.Interfaces;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main;
using Internal.Integration.Core.Models.Main.Product;
using Search.Application.Interfaces.Product;
using Search.Entities;

namespace Search.Persistence.DocumentProviders;

public class MainProductSearchDocumentProvider(
    IMainClient mainClient) : IProductSearchDocumentProvider
{
    public async Task<Product?> GetById(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var fullProduct = await mainClient.ProductNode.GetFullProduct(productId, cancellationToken);
        if (fullProduct == null)
        {
            return null;
        }

        return new Product
        {
            Id = fullProduct.Product.Id,
            Sku = fullProduct.Product.Sku,
            NormalizedSku = fullProduct.Product.Sku.OnlyCharacterToLower(),
            Name = fullProduct.Product.Name,
            ProducerId = fullProduct.Product.ProducerId,
            Dimensions = MapDimensions(fullProduct.ProductSize),
            Weight = MapWeight(fullProduct.ProductWeight),
            Stock = fullProduct.Product.Stock
        };
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
