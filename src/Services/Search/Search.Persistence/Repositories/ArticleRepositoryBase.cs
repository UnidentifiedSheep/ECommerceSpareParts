using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Search.Abstractions.Interfaces.IndexDirectory;
using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.Converters;

namespace Search.Persistence.Repositories;

internal class ArticleRepositoryBase : RepositoryBase, IArticleRepository
{
    public ArticleRepositoryBase(IIndexDirectoryProvider directoryProvider, StandardAnalyzer analyzer) 
        : base(directoryProvider, analyzer, IndexName.Articles) { }

    public void Add(Article article)
    {
        var document = article.ToDocument();
        IndexWriter.UpdateDocument(new Term("Id", article.Id.ToString()), document);
        IndexWriter.Commit();
        OnIndexChanged();
    }
    
    public void AddRange(IEnumerable<Article> articles)
    {
        var documents = articles.Select(a => a.ToDocument());
        IndexWriter.UpdateDocuments(new Term("Id"), documents);
        IndexWriter.Commit();
        OnIndexChanged();
    }

    public Article? GetArticle(int articleId)
    {
        var query = new TermQuery(new Term("Id", articleId.ToString()));
        var topDocs = Searcher.Search(query, 1);

        if (topDocs.TotalHits == 0) return null;

        var doc = Searcher.Doc(topDocs.ScoreDocs[0].Doc);
        return doc.ToArticle();
    }
    
    public List<Article> GetArticles(IEnumerable<int> articleIds)
    {
        var ids = articleIds.Select(id => id.ToString()).ToHashSet();
        if (ids.Count == 0) return [];

        var filter = new TermsFilter(ids.Select(id => new Term("Id", id)).ToList());
        var topDocs = Searcher.Search(new MatchAllDocsQuery(), filter, ids.Count);

        var docs = new List<Article>(topDocs.ScoreDocs.Length);

        foreach (var sd in topDocs.ScoreDocs) docs.Add(Searcher.Doc(sd.Doc).ToArticle());

        return docs;
    }
}