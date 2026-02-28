using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Search.Abstractions.Interfaces.Persistence;
using Search.Enums;

namespace Search.Persistence.IndexContexts;

public abstract class IndexContext : IIndexContext, IDisposable
{
    public abstract IndexName IndexName { get; }
    public Analyzer Analyzer { get; protected set; }
    public FSDirectory Directory { get; protected set; }
    public DirectoryReader Reader { get; protected set; }
    public IndexSearcher Searcher { get; protected set; }
    public IndexWriter IndexWriter { get; protected set; }
    
    private bool _disposed;
    
    protected IndexContext(Analyzer analyzer, FSDirectory directory)
    {
        Analyzer = analyzer;
        Directory = directory;
        var indexConfig = new IndexWriterConfig(Global.LuceneVersion, analyzer);
        IndexWriter = new IndexWriter(Directory, indexConfig);
        Reader = DirectoryReader.Open(IndexWriter, true);
        Searcher = new IndexSearcher(Reader);
    }
    
    /// <summary>
    /// Reloads the index. Should be called after committed changes.
    /// </summary>
    public virtual void ReloadIndex()
    {
        if (_disposed) return;
        var newReader = DirectoryReader.OpenIfChanged(Reader);
        if (newReader == null) return;
        
        Reader.Dispose();
        Reader = newReader;
        Searcher = new IndexSearcher(Reader);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Analyzer.Dispose();
        Reader.Dispose();
        IndexWriter.Dispose();
    }

}