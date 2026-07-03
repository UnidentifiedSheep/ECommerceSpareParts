using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Attributes;
using Contracts.Products;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Main.Enums.Products;

namespace Main.Application.Handlers.Products.CreateProducts;

[AutoSave]
[Transactional]
public record CreateProductsCommand(
    List<CreateProductDto> NewProducts,
    CreateProductsConflictPolicy Policy = CreateProductsConflictPolicy.Fail
) : ICommand<CreateProductsResult>;

public record CreateProductsResult(List<int> CreatedIds, int Skipped = 0);

public class CreateProductsHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IIntegrationEventScope integrationEventScope
)
    : ICommandHandler<CreateProductsCommand, CreateProductsResult>
{
    public async Task<CreateProductsResult> Handle(
        CreateProductsCommand request,
        CancellationToken cancellationToken)
    {
        var newProducts = await GetProductsToCreate(request, cancellationToken);
        var products = new List<Product>();

        foreach (var @new in newProducts)
        {
            var product = Product.Create(
                @new.Sku,
                @new.Name,
                @new.ProducerId,
                @new.Description);
            product.SetIndicator(@new.Indicator);
            product.SetCategory(@new.CategoryId);
            products.Add(product);
        }

        await unitOfWork.AddRangeAsync(products, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await PublishEvent(products, cancellationToken);

        return new CreateProductsResult(
            products.Select(x => x.Id).ToList(),
            request.Policy == CreateProductsConflictPolicy.SkipExisting
                ? request.NewProducts.Count - products.Count
                : 0);
    }

    private async Task<List<CreateProductDto>> GetProductsToCreate(
        CreateProductsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Policy == CreateProductsConflictPolicy.Fail) return request.NewProducts;

        var keys = request.NewProducts
            .Select(GetProductKey)
            .ToList();

        var existingKeys = await productRepository
            .GetExistingProductKeys(keys, cancellationToken);
        var currentBatchKeys = new HashSet<(string NormalizedSku, int ProducerId)>();
        var products = new List<CreateProductDto>();

        foreach (var newProduct in request.NewProducts)
        {
            var key = GetProductKey(newProduct);

            if (existingKeys.Contains(key)) continue;

            if (!currentBatchKeys.Add(key)) continue;

            products.Add(newProduct);
        }

        return products;
    }

    private static (string NormalizedSku, int ProducerId) GetProductKey(CreateProductDto product)
    {
        return (new Sku(product.Sku).NormalizedValue, product.ProducerId);
    }

    private async Task PublishEvent(List<Product> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
            integrationEventScope.Add(
                new ProductUpdatedEvent
                {
                    Id = product.Id
                });

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}