using Core.Extensions;
using MonoliteUnicorn.Dtos.Amw.Articles;
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
        
        new ArticleFullDto().Map("count", x => x.CurrentStock);
        
        new Producer().MapDefault(x => x.Id)
            .Map("id", x => x.Id)
            .Map("name", x => x.Name);
        new Purchase().MapDefault(x => x.PurchaseDatetime)
            .Map("dateTime", x => x.PurchaseDatetime)
            .Map("totalSum", x => x.Transaction.TransactionSum)
            .Map("id", x => x.Id);

        new StorageContentReservation()
            .MapDefault(x => x.CreateAt)
            .Map("id", x => x.Id)
            .Map("createAt", x => x.CreateAt)
            .Map("updatedAt", x => x.UpdatedAt)
            .Map("isDone", x => x.IsDone);
    }
}