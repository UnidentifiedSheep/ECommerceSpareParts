using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.Converters;
using Search.Persistence.Enumerators;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Repositories;

internal class ArticleReadRepository(IIndexManager indexManager) : RepositoryBase(indexManager, IndexName.Articles), IArticleReadRepository
{
    private IndexSearcher Searcher => IndexContext.Searcher;
    
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

    public ArticleEnumerator GetEnumerator() => new(this);
    public Article? GetNextArticle(int articleId)
    {
        var query = new TermQuery(new Term("Id", articleId.ToString()));
        var topDocs = Searcher.Search(query, 1);
        if (topDocs.TotalHits == 0) return null;
        
        var doc = Searcher.Doc(topDocs.ScoreDocs[0].Doc);
        return doc.ToArticle();
    }
}