using System.Linq.Expressions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Projections;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Extensions.QueryExtensions;

public static class ProductQueryExtensions
{
	public static IQueryable<ProductDto> SelectProductDto(
		this IQueryable<Product> query,
		IProjectionProvider<Product, ProductDto> projection) => query.Project(projection);

	public static Task<ProductDto?> FirstProductDtoAsync(
		this IQueryable<Product> query,
		IProjectionProvider<Product, ProductDto> projection,
		Expression<Func<Product, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
	{
		return query
			.WithPredicate(predicate)
			.SelectProductDto(projection)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public static Task<Dictionary<int, ProductDto>> DictionaryProductDto(
		this IQueryable<Product> query,
		IProjectionProvider<Product, ProductDto> projection,
		Expression<Func<Product, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
	{
		return query
			.WithPredicate(predicate)
			.SelectProductDto(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);
	}

	private static IQueryable<Product> WithPredicate(
		this IQueryable<Product> query,
		Expression<Func<Product, bool>>? predicate) => predicate == null ? query : query.Where(predicate);
}
