using System.Linq.Expressions;

namespace Application.Common.Interfaces.Projections;

public abstract class ProjectionProviderBase<TIn, TOut>
    : IProjectionProvider<TIn, TOut>
{
    private readonly Lazy<Func<TIn, TOut>> _projectionFunc;

    protected ProjectionProviderBase()
    {
        _projectionFunc = new Lazy<Func<TIn, TOut>>(
            () => Projection.Compile(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public abstract Expression<Func<TIn, TOut>> Projection { get; }

    public Func<TIn, TOut> ProjectionFunc => _projectionFunc.Value;
}
