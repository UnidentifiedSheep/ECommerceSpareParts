using System.Linq.Expressions;

namespace Abstractions.Models.SortyBy;

public sealed record KeySelectorSortDefinition<TEntity>(
    Expression<Func<TEntity,object?>> KeySelector, 
    bool Desc);