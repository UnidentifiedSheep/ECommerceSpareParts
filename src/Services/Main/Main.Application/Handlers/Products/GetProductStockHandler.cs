using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetProductStockQuery(
    int ProductId, 
    string? StorageName) : IQuery<GetProductStockResult>;
public record GetProductStockResult(int Stock);

public class GetProductStockHandler(
    IReadRepository<Product, int> productReadRepository,
    IReadRepository<StorageContent, int> storageContentReadRepository) : IQueryHandler<GetProductStockQuery, GetProductStockResult>
{
    public async Task<GetProductStockResult> Handle(
        GetProductStockQuery request, 
        CancellationToken cancellationToken)
    {
        int result = string.IsNullOrWhiteSpace(request.StorageName) 
            ? await productReadRepository.Query
                .Where(x => x.Id == request.ProductId)
                .Select(x => x.Stock.Value)
                .FirstOrDefaultAsync(cancellationToken)
            : await storageContentReadRepository.Query
                .Where(x => 
                    x.StorageName == request.StorageName && 
                    x.ProductId == request.ProductId &&
                    x.Count > 0)
                .SumAsync(x => x.Count, cancellationToken);
        
        
        return new GetProductStockResult(result);
    }
}