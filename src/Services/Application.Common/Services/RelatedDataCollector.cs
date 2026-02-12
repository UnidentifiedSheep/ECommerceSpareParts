using Application.Common.Interfaces;

namespace Application.Common.Services;

public class RelatedDataCollector : IRelatedDataCollector
{
    private readonly Stack<HashSet<string>> _stack = new();

    public IDisposable BeginScope()
    {
        _stack.Push([]);
        return new Scope(this);
    }

    public void Add(string id) => _stack.Peek().Add(id);
    public void AddRange(IEnumerable<string> ids) => _stack.Peek().UnionWith(ids);

    public IReadOnlyCollection<string> CurrentIds => _stack.Peek();

    private void EndScope() => _stack.Pop();

    private class Scope(RelatedDataCollector collector) : IDisposable
    {
        public void Dispose() => collector.EndScope();
    }
}