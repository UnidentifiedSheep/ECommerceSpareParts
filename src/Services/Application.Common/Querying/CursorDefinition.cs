using System.Linq.Expressions;

namespace Application.Common.Querying;

public sealed record CursorDefinition<TEntity, TKey>(Expression<Func<TEntity, TKey>> KeySelector, bool Desc)
	where TKey : struct;
