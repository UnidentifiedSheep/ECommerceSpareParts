using Abstractions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Querying;
using Exceptions;

namespace Application.Common.Extensions;

public static class SortByExtensions
{
	public static IOrderedQueryable<TEntity> SortBy<TEntity>(
		this IQueryable<TEntity> query,
		IReadOnlyCollection<string>? sortParams)
	{
		var sorts = ParseSorts<TEntity>(sortParams);
		var first = sorts[0];
		var ordered = first.Desc
			? query.OrderByDescending(first.KeySelector)
			: query.OrderBy(first.KeySelector);

		return sorts
			.Skip(1)
			.Aggregate(
				ordered,
				static (current, sort) => sort.Desc
					? current.ThenByDescending(sort.KeySelector)
					: current.ThenBy(sort.KeySelector));
	}

	public static IOrderedQueryable<TEntity> ThenSortBy<TEntity>(
		this IOrderedQueryable<TEntity> query,
		IReadOnlyCollection<string>? sortParams)
	{
		return ParseSorts<TEntity>(sortParams)
			.Aggregate(
				query,
				static (current, sort) => sort.Desc
					? current.ThenByDescending(sort.KeySelector)
					: current.ThenBy(sort.KeySelector));
	}

	public static CriteriaBuilder<TEntity> WithSorting<TEntity>(
		this CriteriaBuilder<TEntity> builder,
		IReadOnlyCollection<string>? sortParams) where TEntity : class
	{
		foreach (var sort in ParseSorts<TEntity>(sortParams))
			if (sort.Desc)
				builder.OrderByDesc(sort.KeySelector);
			else
				builder.OrderByAsc(sort.KeySelector);

		return builder;
	}

	private static IReadOnlyList<KeySelectorSortDefinition<TEntity>> ParseSorts<TEntity>(
		IReadOnlyCollection<string>? sortParams)
	{
		try
		{
			return QueryableSortBy.ParseToKeySelectors<TEntity>(sortParams);
		}
		catch (ArgumentException exception)
		{
			throw new InvalidInputException(
				SortingInvalidMessage.Create(exception.Message),
				exception.Message);
		}
	}
}
