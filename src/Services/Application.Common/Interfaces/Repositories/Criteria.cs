using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories;

public sealed class Criteria<T> where T : class
{
    public Expression<Func<T, bool>>? Where { get; init; }
    public List<Expression<Func<T, object>>> Includes { get; init; } = new();
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; init; }
    
    public int? Page { get; init; }
    public int? Size { get; init; }
    
    public bool Track { get; init; }
    public bool ForUpdate { get; init; }

    public static CriteriaBuilder<T> New() => new();
}