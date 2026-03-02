using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Search.Enums;
using Search.Persistence.Analyzers;
using Search.Persistence.Interfaces.IndexDirectory;

namespace Search.Persistence.IndexContexts;

internal sealed class ArticleIndexContext : IndexContext
{
    public DirectoryReader Reader 
    {
        get
        {
            ThrowIfDisposedOrClosed();
            return field;
        }
        private set; 
    }
    public IndexSearcher Searcher 
    {
        get
        {
            ThrowIfDisposedOrClosed();
            return field;
        }
        private set; 
    }
    public IndexWriter IndexWriter 
    {
        get
        {
            ThrowIfDisposedOrClosed();
            return field;
        }
        private set; 
    }
    
    public override IndexName IndexName => IndexName.Articles;

    public ArticleIndexContext(ArticleAnalyzer analyzer, IIndexDirectory indexDir) : base(analyzer)
    {
        Path = indexDir.GetIndexPath(IndexName);
        Directory = FSDirectory.Open(Path);
        var indexConfig = new IndexWriterConfig(Global.LuceneVersion, analyzer);
        IndexWriter = new IndexWriter(Directory, indexConfig);
        Reader = DirectoryReader.Open(IndexWriter, true);
        Searcher = new IndexSearcher(Reader);
    }
    
    public void ReloadIndex()
    {
        ThrowIfDisposedOrClosed();
        var newReader = DirectoryReader.OpenIfChanged(Reader);
        if (newReader == null) return;
        
        Reader.Dispose();
        Reader = newReader;
        Searcher = new IndexSearcher(Reader);
    }

    public override void Close()
    {
        ObjectDisposedException.ThrowIf(Disposed, nameof(ArticleIndexContext));
        if (IsClosed) return;
        
        Reader.Dispose();
        IndexWriter.Dispose();
        Directory.Dispose();
        
        IsClosed = true;
    }

    public override void Open()
    {
        ObjectDisposedException.ThrowIf(Disposed, nameof(ArticleIndexContext));
        if (!IsClosed) return;
        
        Directory = FSDirectory.Open(Path);
        var indexConfig = new IndexWriterConfig(Global.LuceneVersion, Analyzer);
        IndexWriter = new IndexWriter(Directory, indexConfig);
        Reader = DirectoryReader.Open(IndexWriter, true);
        Searcher = new IndexSearcher(Reader);
        
        IsClosed = false;
    }

    public override void Dispose()
    {
        Close();
        Disposed = true;
    }
}