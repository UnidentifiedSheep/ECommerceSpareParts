using Lucene.Net.Documents;
using Search.Entities;

namespace Search.Persistence.Extensions;

public static class DocumentExtensions
{
    public static Document ToDocument(this Product source)
    {
        return
        [
            new Int32Field("Id", source.Id, Field.Store.YES),
            new StringField("IdString", source.Id.ToString(), Field.Store.NO),
            new StringField("Sku", source.Sku, Field.Store.YES),
            new TextField("NormalizedSku", source.NormalizedSku, Field.Store.YES),
            new TextField("Title", source.Title, Field.Store.YES),
            new Int32Field("ProducerId", source.ProducerId, Field.Store.YES),
            new StringField("ProducerName", source.ProducerName, Field.Store.YES),
            new Int64Field("Popularity", source.Popularity, Field.Store.YES)
        ];
    }

    public static Product ToArticle(this Document doc)
    {
        var id = doc.GetField("Id").GetInt32Value() ?? 0;
        var sku = doc.GetField("Sku").GetStringValue();
        var normalizedSku = doc.GetField("NormalizedSku").GetStringValue();
        var title = doc.GetField("Title")?.GetStringValue() ?? "";
        var producerId = doc.GetField("ProducerId")?.GetInt32Value() ?? 0;
        var producerName = doc.GetField("ProducerName")?.GetStringValue() ?? "";
        var popularity = doc.GetField("Popularity")?.GetInt64Value() ?? 0;

        return new Product(id, sku, normalizedSku, title, producerId, producerName, popularity);
    }


    public static List<Product> ToArticles(this IEnumerable<Document> documents)
    {
        return documents.Select(ToArticle).ToList();
    }
}