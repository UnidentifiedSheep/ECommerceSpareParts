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
		IReadOnlyCollection<string>? sortBy,
		string? stockStorageCode)
	{
		if (string.IsNullOrWhiteSpace(stockStorageCode))
			return search.SortBySearchRelevance(sortBy, product => product.Id);
		if (sortBy is null || sortBy.All(string.IsNullOrWhiteSpace))
			return search.SortBySearchRelevance(sortBy, product => product.Id);

		return search.Sort(sort =>
		{
			foreach (var definition in ParseSortDefinitions<Product>(sortBy))
			{
				var field = Infer.Field(definition.KeySelector);
				var order = definition.Desc ? SortOrder.Descending : SortOrder.Ascending;
				var member = definition.KeySelector.Body is UnaryExpression conversion
					? conversion.Operand
					: definition.KeySelector.Body;
				sort = member is MemberExpression { Member.Name: nameof(Product.Stock) }
					? sort.Field(value => value
						.Field($"stocks.{ProductStockFields.GetStorageKey(stockStorageCode)}")
						.Order(order).Missing(0).UnmappedType(FieldType.Long))
					: sort.Field(value => value.Field(field).Order(order));
			}
			return sort.Ascending(product => product.Id);
		});
	}

	public static SearchDescriptor<TEntity> SortBySearchRelevance<TEntity, TKey>(
		this SearchDescriptor<TEntity> search,
		IReadOnlyCollection<string>? sortBy,
		Expression<Func<TEntity, TKey>> idSelector) where TEntity : class
	{
		if (sortBy is not null && sortBy.Any(x => !string.IsNullOrWhiteSpace(x)))
			return search.SortBy(sortBy);

		return search.Sort(sort => sort.Descending(SortSpecialField.Score).Ascending(idSelector));
	}

	public static SearchDescriptor<TEntity> SortBy<TEntity>(
		this SearchDescriptor<TEntity> search,
		IReadOnlyCollection<string>? sortBy,
		bool useDefault = true) where TEntity : class
	{
		if (!useDefault && (sortBy is null || sortBy.All(string.IsNullOrWhiteSpace)))
			return search;

		return search.Sort(sort => sort.SortBy(sortBy));
	}

	public static IPromise<IList<ISort>> SortBy<TEntity>(
		this SortDescriptor<TEntity> sort,
		IReadOnlyCollection<string>? sortBy)
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
		IReadOnlyCollection<string>? sortBy) where TEntity : class
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
