using Abstractions;
using Abstractions.Models.SortyBy;
using Application.Common.Interfaces.Repositories;
using Exceptions;

namespace Application.Common.Extensions;

public static class SortByExtensions
{
	public static IOrderedQueryable<TEntity> SortBy<TEntity>(
		this IQueryable<TEntity> query,
		string[]? sortParams)
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
		string[]? sortParams)
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
		string[]? sortParams) where TEntity : class
	{
		foreach (var sort in ParseSorts<TEntity>(sortParams))
			if (sort.Desc)
				builder.OrderByDesc(sort.KeySelector);
			else
				builder.OrderByAsc(sort.KeySelector);

		return builder;
	}

	private static IReadOnlyList<KeySelectorSortDefinition<TEntity>> ParseSorts<TEntity>(string[]? sortParams)
	{
		try
		{
			return QueryableSortBy.ParseToKeySelectors<TEntity>(sortParams);
		}
		catch (ArgumentException exception)
		{
			throw new InvalidInputException(
				"sorting.invalid",
				[exception.Message],
				exception.Message);
		}
	}
}
