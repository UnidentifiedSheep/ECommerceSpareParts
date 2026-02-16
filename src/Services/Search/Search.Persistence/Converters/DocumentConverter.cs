using Lucene.Net.Documents;
using Search.Entities;

namespace Search.Persistence.Converters;

public static class DocumentConverter
{
    public static Document ToDocument(this Article source)
    {
        return
        [
            new Int32Field("Id", source.Id, Field.Store.YES),
            new StringField("Id", source.Id.ToString(), Field.Store.NO),
            new StringField("ArticleNumber", source.ArticleNumber, Field.Store.YES),
            new TextField("Title", source.Title, Field.Store.YES),
            new Int32Field("ProducerId", source.ProducerId, Field.Store.YES),
            new StringField("ProducerName", source.ProducerName, Field.Store.YES),
            new Int64Field("Popularity", source.Popularity, Field.Store.YES)
        ];
    }

    public static Article ToArticle(this Document doc)
    {
        var id = doc.GetField("Id").GetInt32Value() ?? 0;
        var articleNumber = doc.GetField("ArticleNumber").GetStringValue();
        var title = doc.GetField("Title")?.GetStringValue() ?? "";
        var producerId = doc.GetField("ProducerId")?.GetInt32Value() ?? 0;
        var producerName = doc.GetField("ProducerName")?.GetStringValue() ?? "";
        var popularity = doc.GetField("Popularity")?.GetInt64Value() ?? 0;

        return new Article(id, articleNumber, title, producerId, producerName, popularity);
    }

    
    public static List<Article> ToArticles(this IEnumerable<Document> documents) => documents.Select(ToArticle).ToList();
}