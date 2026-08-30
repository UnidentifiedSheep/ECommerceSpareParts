using System.Linq.Expressions;

namespace Abstractions.Models;

public sealed record CursorDefinition<TEntity, TKey>(Expression<Func<TEntity, TKey>> KeySelector, bool Desc)
	where TKey : struct;
