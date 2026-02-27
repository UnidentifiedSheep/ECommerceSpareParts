using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Search.Persistence.Models;

public sealed class IndexContext : IDisposable
{
    public StandardAnalyzer Analyzer { get; private set; }
    public FSDirectory Directory { get; private set; }
    public DirectoryReader Reader { get; private set; }
    public IndexSearcher Searcher { get; private set; }
    public IndexWriter IndexWriter { get; private set; }
    
    private bool _disposed;
    
    public IndexContext(StandardAnalyzer analyzer, FSDirectory directory)
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
    public void ReloadIndex()
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