using System.Linq.Expressions;
using Abstractions;
using Application.Common;
using Application.Common.Querying;
using Exceptions;
using OpenSearch.Client;
using Search.Entities;
using Search.Persistence.Models;

namespace Search.Persistence.Extensions;

public static class SortDescriptorExtensions
{
	public static SearchDescriptor<Product> SortProductsBySearchRelevance(
		this SearchDescriptor<Product> search,
		IReadOnlyCollection<SortExpression>? sortBy)
	{
		if (sortBy is null || sortBy.Count == 0)
			return search.SortBySearchRelevance(sortBy, product => product.Id);

		var values = sortBy.ToArray();
		var definitions = ParseSortDefinitions<Product>(values);
		return search.Sort(sort =>
		{
			for (var i = 0; i < values.Length; i++)
			{
				var definition = definitions[i];
				var order = definition.Desc ? SortOrder.Descending : SortOrder.Ascending;
				if (values[i].Argument is { } storage &&
					string.Equals(values[i].Field.Trim(), "stock", StringComparison.OrdinalIgnoreCase))
				{
					if (string.IsNullOrWhiteSpace(storage))
						throw new InvalidInputException(SortingInvalidMessage.Create("Storage code must not be empty."));
					sort = sort.Field(value => value
						.Field($"stocks.{ProductStockFields.GetStorageKey(storage)}")
						.Order(order).Missing(0).UnmappedType(FieldType.Long));
				}
				else
					sort = sort.Field(value => value.Field(definition.KeySelector).Order(order));
			}
			return sort.Ascending(product => product.Id);
		});
	}

	public static SearchDescriptor<TEntity> SortBySearchRelevance<TEntity, TKey>(
		this SearchDescriptor<TEntity> search,
		IReadOnlyCollection<SortExpression>? sortBy,
		Expression<Func<TEntity, TKey>> idSelector) where TEntity : class
	{
		if (sortBy is not null && sortBy.Count > 0)
			return search.SortBy(sortBy);

		return search.Sort(sort => sort.Descending(SortSpecialField.Score).Ascending(idSelector));
	}

	public static SearchDescriptor<TEntity> SortBy<TEntity>(
		this SearchDescriptor<TEntity> search,
		IReadOnlyCollection<SortExpression>? sortBy,
		bool useDefault = true) where TEntity : class
	{
		if (!useDefault && (sortBy is null || sortBy.Count == 0))
			return search;

		return search.Sort(sort => sort.SortBy(sortBy));
	}

	public static IPromise<IList<ISort>> SortBy<TEntity>(
		this SortDescriptor<TEntity> sort,
		IReadOnlyCollection<SortExpression>? sortBy)
		where TEntity : class
	{
		var definitions = ParseSortDefinitions<TEntity>(sortBy);

		foreach (var definition in definitions)
			sort = definition.Desc
				? sort.Descending(definition.KeySelector)
				: sort.Ascending(definition.KeySelector);

		return sort;
	}

	private static IReadOnlyList<KeySelectorSortDefinition<TEntity>> ParseSortDefinitions<TEntity>(
		IReadOnlyCollection<SortExpression>? sortBy) where TEntity : class
	{
		try
		{
			return QueryableSortBy.ParseToKeySelectors<TEntity>(sortBy);
		}
		catch (ArgumentException exception)
		{
			throw new InvalidInputException(
				new SortingInvalidMessage().WithSort(exception.Message),
				exception.Message);
		}
	}
}
