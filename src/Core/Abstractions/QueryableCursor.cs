using System.Collections.Concurrent;
using System.Linq.Expressions;
using Abstractions.Models;

namespace Abstractions;

public sealed class QueryableCursor
{
	public static readonly QueryableCursor Value = new();

	private readonly ConcurrentDictionary<(Type Source, Type Key), object> _definitions = new();

	public QueryableCursor Map<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector, bool desc = false)
		where TKey : struct
	{
		_definitions[(typeof(TEntity), typeof(TKey))] =
			new CursorDefinition<TEntity, TKey>(keySelector, desc);

		return this;
	}

	public CursorDefinition<TEntity, TKey> GetMapping<TEntity, TKey>() where TKey : struct
	{
		return _definitions.TryGetValue((typeof(TEntity), typeof(TKey)), out var definition)
			? (CursorDefinition<TEntity, TKey>)definition
			: throw new ArgumentException(
				$"Cursor mapping for {typeof(TEntity)} and {typeof(TKey)} does not exist.");
	}
}
