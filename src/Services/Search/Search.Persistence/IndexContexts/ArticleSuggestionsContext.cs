using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Store;
using Search.Enums;
using Search.Persistence.Interfaces.IndexDirectory;

namespace Search.Persistence.IndexContexts;

public class ArticleSuggestionsContext : IndexContext
{
    public ArticleSuggestionsContext(StandardAnalyzer analyzer, IIndexDirectory indexDirectory) : base(analyzer)
    {
        var idxName = IndexName.Article_Suggestions;
        IndexName = idxName;
        Path = indexDirectory.GetIndexPath(idxName);
        Directory = FSDirectory.Open(Path);
        Suggester = new AnalyzingInfixSuggester(Global.LuceneVersion, Directory, Analyzer);
    }

    public AnalyzingInfixSuggester Suggester
    {
        get
        {
            ThrowIfDisposedOrClosed();
            return field;
        }
        private set;
    }

    public override IndexName IndexName { get; }

    public override void Close()
    {
        ObjectDisposedException.ThrowIf(Disposed, nameof(ArticleSuggestionsContext));
        if (IsClosed) return;
        Suggester.Dispose();
        Directory.Dispose();
        IsClosed = true;
    }

    public override void Open()
    {
        ObjectDisposedException.ThrowIf(Disposed, nameof(ArticleSuggestionsContext));
        if (!IsClosed) return;
        Directory = FSDirectory.Open(Path);
        Suggester = new AnalyzingInfixSuggester(Global.LuceneVersion, Directory, Analyzer);
        IsClosed = false;
    }

    public override void Dispose()
    {
        Close();
        Disposed = true;
    }
}