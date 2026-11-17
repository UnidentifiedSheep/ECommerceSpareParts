using System.Linq.Expressions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Projections;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Extensions.QueryExtensions;

public static class ProductQueryExtensions
{
	public static IQueryable<ProductDto> SelectProductDto(
		this IQueryable<Entities.Product.Product.Product> query,
		IProjectionProvider<Entities.Product.Product.Product, ProductDto> projection)
		=> query.Project(projection);

	public static Task<ProductDto?> FirstProductDtoAsync(
		this IQueryable<Entities.Product.Product.Product> query,
		IProjectionProvider<Entities.Product.Product.Product, ProductDto> projection,
		Expression<Func<Entities.Product.Product.Product, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
		=> query
			.WithPredicate(predicate)
			.SelectProductDto(projection)
			.FirstOrDefaultAsync(cancellationToken);

	public static Task<Dictionary<int, ProductDto>> DictionaryProductDto(
		this IQueryable<Entities.Product.Product.Product> query,
		IProjectionProvider<Entities.Product.Product.Product, ProductDto> projection,
		Expression<Func<Entities.Product.Product.Product, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
		=> query
			.WithPredicate(predicate)
			.SelectProductDto(projection)
			.ToDictionaryAsync(x => x.Id, cancellationToken);

	private static IQueryable<Entities.Product.Product.Product> WithPredicate(
		this IQueryable<Entities.Product.Product.Product> query,
		Expression<Func<Entities.Product.Product.Product, bool>>? predicate)
		=> predicate == null ? query : query.Where(predicate);
}
