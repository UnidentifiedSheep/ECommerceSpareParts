using Abstractions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Querying;
using Exceptions;

namespace Application.Common.Extensions;

public static class SortByExtensions
{
	public static SortExpression[] ToSortExpressions(this IEnumerable<string>? sorting)
	{
		try
		{
			return sorting?.Where(value => !string.IsNullOrWhiteSpace(value))
				.Select(SortExpression.Parse)
				.ToArray() ?? [];
		}
		catch (ArgumentException exception)
		{
			throw new InvalidInputException(SortingInvalidMessage.Create(exception.Message), exception.Message);
		}
	}

	public static IOrderedQueryable<TEntity> SortBy<TEntity>(
		this IQueryable<TEntity> query,
		IReadOnlyCollection<SortExpression>? sortParams)
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
		IReadOnlyCollection<SortExpression>? sortParams)
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
		IReadOnlyCollection<SortExpression>? sortParams) where TEntity : class
	{
		foreach (var sort in ParseSorts<TEntity>(sortParams))
			if (sort.Desc)
				builder.OrderByDesc(sort.KeySelector);
			else
				builder.OrderByAsc(sort.KeySelector);

		return builder;
	}

	private static IReadOnlyList<KeySelectorSortDefinition<TEntity>> ParseSorts<TEntity>(
		IReadOnlyCollection<SortExpression>? sortParams)
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
