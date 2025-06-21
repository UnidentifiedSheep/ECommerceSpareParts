using Core.Extensions;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        new Article().MapDefault(x => x.Id)
            .Map("id", x => x.Id)
            .Map("articleNumber", x => x.ArticleNumber)
            .Map("articleName", x => x.ArticleName)
            .Map("count", x => x.TotalCount)
            .Map("producerName", x => x.Producer.Name);
        new Producer().MapDefault(x => x.Id)
            .Map("id", x => x.Id)
            .Map("name", x => x.Name);
        new Purchase().MapDefault(x => x.PurchaseDatetime)
            .Map("dateTime", x => x.PurchaseDatetime)
            .Map("totalSum", x => x.Transaction.TransactionSum)
            .Map("id", x => x.Id);
    }
}