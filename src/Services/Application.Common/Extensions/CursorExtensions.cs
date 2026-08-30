using System.Globalization;
using System.Linq.Expressions;
using Abstractions;
using Abstractions.Models;

namespace Application.Common.Extensions;

public static class CursorExtensions
{
	public static IQueryable<TEntity> ApplyCursor<TEntity, TKey>(
		this IQueryable<TEntity> query,
		Cursor<TKey?> cursor) where TKey : struct
	{
		var definition = QueryableCursor.Value.GetMapping<TEntity, TKey>();

		if (cursor.CursorValue is { } cursorValue)
			query = query.Where(
				BuildSeekPredicate(
					definition.KeySelector,
					cursorValue,
					definition.Desc));

		var ordered = definition.Desc
			? query.OrderByDescending(definition.KeySelector)
			: query.OrderBy(definition.KeySelector);

		return ordered.Take(cursor.Size);
	}

	public static string? GetNextCursor<TEntity, TKey>(
		this IReadOnlyList<TEntity> items,
		Cursor<TKey?> cursor) where TKey : struct
	{
		if (items.Count == 0)
			return null;

		var definition = QueryableCursor.Value.GetMapping<TEntity, TKey>();
		var cursorValue = definition.KeySelector.Compile()(items[^1]);

		return cursorValue switch
		{
			DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
			DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
			IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
			_ => cursorValue.ToString()
		};
	}

	private static Expression<Func<TEntity, bool>> BuildSeekPredicate<TEntity, TKey>(
		Expression<Func<TEntity, TKey>> keySelector,
		TKey cursorValue,
		bool desc) where TKey : struct
	{
		var cursor = Expression.Constant(cursorValue, typeof(TKey));
		var comparison = desc
			? Expression.LessThan(keySelector.Body, cursor)
			: Expression.GreaterThan(keySelector.Body, cursor);

		return Expression.Lambda<Func<TEntity, bool>>(comparison, keySelector.Parameters);
	}
}
