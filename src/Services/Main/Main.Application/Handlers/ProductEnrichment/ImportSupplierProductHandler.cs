using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Models.Supplier;
using Enums;
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
    IRepository<SupplierProduct, int> repository,
    IUnitOfWork unitOfWork,
    ILogger<ImportSupplierProductHandler> logger
    ) : ICommandHandler<ImportSupplierProductCommand>
{
    public async Task<Unit> Handle(
        ImportSupplierProductCommand request,
        CancellationToken cancellationToken)
    {
        var products = request.Products
            .Select(x => TryCreateImportProduct(x, out var product) ? product : null)
            .OfType<ImportProduct>()
            .ToList();

        var existingProducts = await GetExistingProducts(
            request.Supplier,
            products.Select(x => x.Key).ToHashSet(),
            cancellationToken);

        var toAdd = new List<SupplierProduct>();

        foreach (var product in products)
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
        return Unit.Value;
    }

    private async Task<Dictionary<SupplierProductKey, SupplierProduct>> GetExistingProducts(
        Supplier supplier,
        IReadOnlyCollection<SupplierProductKey> keys,
        CancellationToken cancellationToken)
    {
        if (keys.Count == 0) return [];

        var producers = keys.Select(x => x.Producer).Distinct().ToList();
        var numbers = keys.Select(x => x.Sku).Distinct().ToList();

        return (await repository.ListAsync(
                Criteria<SupplierProduct>.New()
                    .Include(x => x.Names)
                    .Where(x => x.Supplier == supplier)
                    .Where(x => producers.Contains(x.Producer))
                    .Where(x => numbers.Contains(x.Sku.NormalizedValue))
                    .Track()
                    .Build(),
                cancellationToken))
            .ToDictionary(x => new SupplierProductKey(x.Sku.NormalizedValue, x.Producer));
    }

    private bool TryCreateImportProduct(
        ContractSupplierProductDto product,
        out ImportProduct? importProduct)
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
    private sealed record ImportProduct(
        SupplierProductKey Key,
        ContractSupplierProductDto Product);
}
