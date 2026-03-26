using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Search.Abstractions.Interfaces.Persistence;
using Search.Enums;

namespace Search.Persistence.IndexContexts;

public abstract class IndexContext(Analyzer analyzer) : IIndexContext, IDisposable
{
    public string Path { get; protected set; } = string.Empty; //should be initialized in child classes
    public Analyzer Analyzer { get; protected set; } = analyzer;
    public FSDirectory Directory { get; protected set; } = null!; //should be initialized in child classes
    public bool IsClosed { get; protected set; }
    public bool Disposed { get; protected set; }
    public abstract void Dispose();
    public abstract IndexName IndexName { get; }

    public abstract void Close();
    public abstract void Open();

    protected void ThrowIfDisposedOrClosed()
    {
        if (IsClosed) throw new InvalidOperationException("Context is closed. Use Open() to reopen it.");
        ObjectDisposedException.ThrowIf(Disposed, this);
    }
}