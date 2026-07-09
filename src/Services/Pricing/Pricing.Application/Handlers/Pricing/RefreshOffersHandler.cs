using Application.Common.Interfaces.Cqrs;
using Attributes;
using Enums;
using Integrations.Supplier.Models;
using Internal.Integration.Core.Interfaces.Main;
using Internal.Integration.Core.Models.Main.Product;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Pricing;

[Diagnostics]
[Transactional, AutoSave]
public record RefreshOffersCommand : ICommand<RefreshOffersResult>
{
    public int ProductId { get; }
    public string StorageName { get; }
    
    public IReadOnlyList<SupplierProduct>? Products { get; }
    public Supplier? Supplier { get; }
    public DateTime? DataExtractionTime { get; }
    
    public RefreshOffersCommand(int productId, string storageName)
    {
        ProductId = productId;
        StorageName = storageName;
    }

    public RefreshOffersCommand(
        DateTime dataExtractionTime,
        Supplier supplier,
        string storageName,
        IEnumerable<SupplierProduct> products)
    {
        DataExtractionTime = dataExtractionTime;
        ProductId = -1;
        StorageName = storageName;
        Supplier = supplier;
        Products = products.ToList();
    }
}

public record RefreshOffersResult(IReadOnlyList<PriceOffer> CreatedOffers);

public class RefreshOffersHandler(
    IOfferRefreshService refreshService,
    IMainClient mainClient
    ) : ICommandHandler<RefreshOffersCommand, RefreshOffersResult>
{
    public async Task<RefreshOffersResult> Handle(RefreshOffersCommand request, CancellationToken cancellationToken)
    {
        if (request.Products == null || request.Supplier == null)
            return await BasicRefreshWay(request.ProductId, request.StorageName, cancellationToken);
        
        var dict = new Dictionary<ProductKey, List<SupplierPosition>>();
        var queue = new Queue<SupplierProduct>(request.Products);

        if (queue.Count == 0) return new RefreshOffersResult([]);
        
        
        while (queue.TryDequeue(out var product))
        {
            var key = new ProductKey(product.Brand, product.Number);

            if (!dict.TryAdd(key, product.Positions.ToList()))
                continue;

            foreach (var analogue in product.Analogues)
                queue.Enqueue(analogue);
        }

        var productIds = await ResolveProductIdsAsync(
            request.Supplier.Value,
            dict,
            cancellationToken);
        
        var refreshDict = dict
            .Select(x => (
                ProductId: productIds.TryGetValue(x.Key, out var productId) ? productId : (int?)null,
                Positions: (IReadOnlyList<SupplierPosition>)x.Value.ToList()))
            .Where(x => x.ProductId != null)
            .ToDictionary(x => x.ProductId!.Value, x => x.Positions);
        
        var offers = await refreshService.RefreshOffersAsync(
            request.DataExtractionTime!.Value,
            request.StorageName,
            request.Supplier.Value,
            refreshDict,
            cancellationToken);
        
        return new RefreshOffersResult(offers);
    }

    private readonly record struct ProductKey(string Brand, string Number);

    private async Task<Dictionary<ProductKey, int>> ResolveProductIdsAsync(
        Supplier supplier,
        Dictionary<ProductKey,List<SupplierPosition>> dict,
        CancellationToken cancellationToken)
    {
        var requestDict = new Dictionary<Supplier, IEnumerable<InternalSupplierProductReferenceLookup>>()
        {
            [supplier] = dict.Keys
                .Select(x => new InternalSupplierProductReferenceLookup
                {
                    Sku = x.Number,
                    SupplierProducerName = x.Brand
                })
        };
        
        var response = await mainClient.ProductNode
            .ResolveSupplierProductReferences(
                requestDict,
                cancellationToken);

        if (!response.Success)
            throw new InvalidOperationException("Unable to resolve product ids, from main service");

        if (!response.ValueOrThrow.TryGetValue(supplier, out var result))
            return new Dictionary<ProductKey, int>();

        return result.ToDictionary(
            x => new ProductKey(x.SupplierProducerName, x.Sku),
            x => x.ProductId);
    }
    
    private async Task<RefreshOffersResult> BasicRefreshWay(
        int productId,
        string storageName,
        CancellationToken cancellationToken)
    {
        var offers = await refreshService
            .RefreshOffersAsync(
                productId, 
                storageName, 
                cancellationToken);
            
        return new RefreshOffersResult(offers);
    }
}
