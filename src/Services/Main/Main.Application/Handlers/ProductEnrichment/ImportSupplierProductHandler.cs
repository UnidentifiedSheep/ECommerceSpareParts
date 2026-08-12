using System.Data;
using System.Diagnostics.CodeAnalysis;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Contracts.Models.Supplier;
using Enums;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product.Enrichment;
using Main.Entities.Product.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Main.Application.Handlers.ProductEnrichment;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 40, 3, "23505")]
public record ImportSupplierProductCommand(
    Supplier Supplier,
    IReadOnlyCollection<ContractSupplierProductDto> Products
) : ICommand;

public class ImportSupplierProductHandler(
    ISupplierProductRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ImportSupplierProductHandler> logger
    ) : ICommandHandler<ImportSupplierProductCommand>
{
    public async Task<Unit> Handle(
        ImportSupplierProductCommand request,
        CancellationToken cancellationToken)
    {
        var graph = BuildImportGraph(request.Products);

        var existingProducts = await GetExistingProducts(
            request.Supplier,
            graph.Products.Select(x => x.Key).ToHashSet(),
            cancellationToken);

        var toAdd = new List<SupplierProduct>();

        foreach (var product in graph.Products)
        {
            var requestProduct = product.Product;
            if (!existingProducts.TryGetValue(product.Key, out var existingProduct))
            {
                existingProduct = SupplierProduct.Create(
                    requestProduct.Number,
                    requestProduct.Brand,
                    request.Supplier);

                existingProducts.Add(product.Key, existingProduct);
                toAdd.Add(existingProduct);
            }

            foreach (var name in requestProduct.Names.Where(x => !string.IsNullOrWhiteSpace(x)))
                existingProduct.AddName(name);
        }

        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var crosses = graph.Crosses
            .Select(x => SupplierProductCross.Create(
                existingProducts[x.Product].Id,
                existingProducts[x.Analogue].Id))
            .DistinctBy(x => x.GetId())
            .ToList();

        await repository.UpsertCrossesAsync(crosses, cancellationToken);
        return Unit.Value;
    }

    private async Task<Dictionary<SupplierProductKey, SupplierProduct>> GetExistingProducts(
        Supplier supplier,
        IReadOnlyCollection<SupplierProductKey> keys,
        CancellationToken cancellationToken)
    {
        if (keys.Count == 0) return [];

        return (await repository.GetBySupplierKeysAsync(
                supplier,
                keys.Select(x => (x.Sku, x.Producer)),
                cancellationToken))
            .ToDictionary(x => new SupplierProductKey(x.Sku.NormalizedValue, x.Producer));
    }

    private ImportGraph BuildImportGraph(
        IEnumerable<ContractSupplierProductDto> rootProducts)
    {
        var products = new List<ImportProduct>();
        var crosses = new List<RequestedCross>();
        var processed = new Dictionary<ContractSupplierProductDto, ImportProduct>(
            ReferenceEqualityComparer.Instance);
        var visited = new HashSet<ContractSupplierProductDto>(
            ReferenceEqualityComparer.Instance);
        var pending = new Queue<(ContractSupplierProductDto Product, SupplierProductKey? Parent)>();

        foreach (var product in rootProducts)
            pending.Enqueue((product, null));

        while (pending.TryDequeue(out var item))
        {
            ImportProduct? product;

            if (visited.Add(item.Product))
            {
                if (TryCreateImportProduct(item.Product, out product))
                {
                    processed.Add(item.Product, product);
                    products.Add(product);
                }

                foreach (var analogue in item.Product.Analogues)
                    pending.Enqueue((analogue, product?.Key));
            }
            else
                processed.TryGetValue(item.Product, out product);

            if (product is not null)
            {
                if (item.Parent is { } parent && parent != product.Key)
                    crosses.Add(new RequestedCross(parent, product.Key));
            }
        }

        return new ImportGraph(products, crosses);
    }

    private bool TryCreateImportProduct(
        ContractSupplierProductDto product,
        [NotNullWhen(true)] out ImportProduct? importProduct)
    {
        importProduct = null;

        if (!Sku.IsValid(product.Number, out var exception))
        {
            logger.LogInformation(
                exception,
                "Skipping supplier product with invalid number: {SupplierProductNumber}",
                product.Number);
            return false;
        }

        if (string.IsNullOrWhiteSpace(product.Brand))
        {
            logger.LogInformation(
                "Skipping supplier product {SupplierProductNumber} with an empty brand",
                product.Number);
            return false;
        }

        var key = new SupplierProductKey(
            Sku.ToNormalized(product.Number),
            product.Brand.Trim());

        importProduct = new ImportProduct(key, product);
        return true;
    }

    private record struct SupplierProductKey(string Sku, string Producer);
    private record struct RequestedCross(
        SupplierProductKey Product,
        SupplierProductKey Analogue);
    private sealed record ImportProduct(
        SupplierProductKey Key,
        ContractSupplierProductDto Product);
    private sealed record ImportGraph(
        IReadOnlyList<ImportProduct> Products,
        IReadOnlyList<RequestedCross> Crosses);
}
