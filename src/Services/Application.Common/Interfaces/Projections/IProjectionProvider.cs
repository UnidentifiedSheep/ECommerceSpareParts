using System.Linq.Expressions;

namespace Application.Common.Interfaces.Projections;

public interface IProjectionProvider<TIn, TOut>
{
    Expression<Func<TIn, TOut>> Projection { get; }
    Func<TIn, TOut> ProjectionFunc { get; }
}
