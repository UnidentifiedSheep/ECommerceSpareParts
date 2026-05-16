using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Projections;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Extensions.QueryExtensions;

public static class ProductQueryExtensions
{
    public static IQueryable<ProductDto> SelectProductDto(
        this IQueryable<Product> query)
    {
        return query
            .AsExpandable()
            .Select(ProductProjections.ToDto);
    }

    public static Task<ProductDto?> FirstProductDtoAsync(
        this IQueryable<Product> query,
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return query
            .WithPredicate(predicate)
            .SelectProductDto()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static Task<Dictionary<int, ProductDto>> DictionaryProductDto(
        this IQueryable<Product> query,
        Expression<Func<Product, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return query
            .WithPredicate(predicate)
            .SelectProductDto()
            .ToDictionaryAsync(x => x.Id, cancellationToken);
    }

    private static IQueryable<Product> WithPredicate(
        this IQueryable<Product> query,
        Expression<Func<Product, bool>>? predicate)
    {
        return predicate == null ? query : query.Where(predicate);
    }
}