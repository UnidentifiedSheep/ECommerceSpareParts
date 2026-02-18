using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Search.Enums;
using Search.Persistence.Interfaces.IndexDirectory;

namespace Search.Persistence.Abstractions;

internal abstract class RepositoryBase : IDisposable
{
    public IndexName IndexName { get; }
    protected StandardAnalyzer Analyzer { get; private set; }
    protected FSDirectory Directory { get; private set; }
    protected DirectoryReader Reader { get; private set; }
    protected IndexSearcher Searcher { get; private set; }
    protected IndexWriter IndexWriter { get; private set;}

    public RepositoryBase(IIndexDirectoryProvider directoryProvider, StandardAnalyzer analyzer, IndexName indexName)
    {
        IndexName = indexName;
        Analyzer = analyzer;
        Directory = directoryProvider.GetDirectory(IndexName);
        var indexConfig = new IndexWriterConfig(Global.LuceneVersion, analyzer);
        IndexWriter = new IndexWriter(Directory, indexConfig);
        Reader = DirectoryReader.Open(IndexWriter, true);
        Searcher = new IndexSearcher(Reader);
    }

    protected void OnIndexChanged()
    {
        var newReader = DirectoryReader.OpenIfChanged(Reader);
        if (newReader == null) return;
        
        Reader.Dispose();
        Reader = newReader;
        Searcher = new IndexSearcher(Reader);
    }

    public virtual void Dispose()
    {
        Directory.Dispose();
        Reader.Dispose();
        IndexWriter.Dispose();
    }
}
