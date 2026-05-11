using Lucene.Net.Index;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.Extensions;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Repositories;

internal class ArticleWriteRepository(IIndexManager indexManager)
    : RepositoryBase<ArticleIndexContext>(indexManager, IndexName.Articles), IArticleWriteRepository
{
    public void Add(Product product)
    {
        var document = product.ToDocument();
        IndexContext.IndexWriter.UpdateDocument(new Term("IdString", product.Id.ToString()), document);
        IndexContext.IndexWriter.Commit();

        IndexContext.ReloadIndex();
    }

    public void AddRange(IEnumerable<Product> articles)
    {
        var documents = articles.Select(a => a.ToDocument());
        IndexContext.IndexWriter.UpdateDocuments(new Term("IdString"), documents);
        IndexContext.IndexWriter.Commit();

        IndexContext.ReloadIndex();
    }

    public void Delete(int articleId)
    {
        IndexContext.IndexWriter.DeleteDocuments(new Term("IdString", articleId.ToString()));
        IndexContext.IndexWriter.Commit();
        IndexContext.ReloadIndex();
    }
}