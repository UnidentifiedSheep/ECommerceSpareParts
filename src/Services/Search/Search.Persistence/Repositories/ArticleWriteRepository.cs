using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Index;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.Extensions;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Repositories;

internal class ArticleWriteRepository(IIndexManager indexManager) : RepositoryBase(indexManager, IndexName.Articles), 
    IArticleWriteRepository
{
    private IndexWriter IndexWriter => IndexContext.IndexWriter;
    public void Add(Article article)
    {
        var document = article.ToDocument();
        IndexWriter.UpdateDocument(new Term("Id", article.Id.ToString()), document);
        IndexWriter.Commit();
        
        IndexContext.ReloadIndex();
    }
    
    public void AddRange(IEnumerable<Article> articles)
    {
        var documents = articles.Select(a => a.ToDocument());
        IndexWriter.UpdateDocuments(new Term("Id"), documents);
        IndexWriter.Commit();
        
        IndexContext.ReloadIndex();
    }

    public void Delete(int articleId)
    {
        IndexWriter.DeleteDocuments(new Term("Id", articleId.ToString()));
        IndexWriter.Commit();
        IndexContext.ReloadIndex();
    }
}