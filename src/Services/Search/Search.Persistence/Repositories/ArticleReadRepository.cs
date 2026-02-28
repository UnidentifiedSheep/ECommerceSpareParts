using Extensions;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Search.Abstractions.Models;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.Enumerators;
using Search.Persistence.Extensions;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Repositories;

internal class ArticleReadRepository(IIndexManager indexManager) : RepositoryBase(indexManager, IndexName.Articles), 
    IArticleReadRepository
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

    public Article? GetNextArticle(int articleId = -1)
    {
        var query = new TermQuery(new Term("Id", articleId.ToString()));
        Filter? filter = GetIdFilter(articleId);
        var sort = SortById();
        var topDocs = Searcher.Search(query, filter, 1, sort);

        if (topDocs.TotalHits == 0) return null;

        var doc = Searcher.Doc(topDocs.ScoreDocs[0].Doc);
        return doc.ToArticle();
    }

    public IReadOnlyList<Article> GetArticles(IEnumerable<int> articleIds)
    {
        var ids = articleIds.Select(id => id.ToString()).ToHashSet();
        if (ids.Count == 0) return [];

        var filter = new TermsFilter(ids.Select(id => new Term("Id", id)).ToList());
        var topDocs = Searcher.Search(new MatchAllDocsQuery(), filter, ids.Count);

        return ToArticles(topDocs);
    }

    public (IReadOnlyList<Article> result, SearchCursor? last) SearchByTitle(string title, SearchCursor? cursor = null, int limit = 20)
    {
        QueryParser parser = new QueryParser(Global.LuceneVersion, "Title", IndexContext.Analyzer);
        Query query = parser.Parse(title);

        TopDocs topDocs = Searcher.SearchAfter(cursor?.ToScoreDoc(), query, limit);

        return ToArticlesWithCursor(topDocs);
    }


    public (IReadOnlyList<Article> result, SearchCursor? last) SearchByArticleNumberPrefix(string prefix, 
        SearchCursor? cursor = null, int limit = 20)
    {
        prefix = prefix.ToNormalizedArticleNumber();
        var query = new PrefixQuery(new Term("NormalizedArticleNumber", prefix));
        
        TopDocs topDocs = Searcher.SearchAfter(cursor?.ToScoreDoc(), query, limit);
        
        return ToArticlesWithCursor(topDocs);
    }
    
    public ArticleEnumerator GetEnumerator() => new(this);

    /// <summary>
    /// Converts the given TopDocs object into a list of Article objects.
    /// </summary>
    private List<Article> ToArticles(TopDocs topDocs)
    {
        var docs = new List<Article>(topDocs.ScoreDocs.Length);
        foreach (var sd in topDocs.ScoreDocs) docs.Add(Searcher.Doc(sd.Doc).ToArticle());
        return docs;
    }

    private (List<Article>, SearchCursor? last) ToArticlesWithCursor(TopDocs topDocs)
    {
        var result = new  List<Article>(topDocs.ScoreDocs.Length);
        ScoreDoc? last = null;

        foreach (var sd in topDocs.ScoreDocs)
        {
            var doc = Searcher.Doc(sd.Doc);
            last = sd;
            result.Add(doc.ToArticle());
        }

        return (result, last?.ToCursor());
    }

    /// <summary>
    /// Returns a filter that selects articles with Id greater than the specified id.
    /// If id is less than 0, no filter is applied.
    /// </summary>
    /// <param name="id">The inclusive lower bound.</param>
    /// <returns>A filter to exclude articles with ids less than the specified value, or null if no filtering is required.</returns>
    private Filter? GetIdFilter(int id)
    {
        if (id < 0) return null;
        return NumericRangeFilter.NewInt32Range("Id", id, int.MaxValue, false, true);
    }

    private Sort SortById(bool descending = false)
    {
        return new Sort(new SortField("Id", SortFieldType.INT32, descending));
    }
}