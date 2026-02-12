using Extensions;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Entities;

namespace Main.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        new Article().MapDefault(x => x.Id)
            .Map("id", x => x.Id)
            .Map("articleNumber", x => x.ArticleNumber)
            .Map("title", x => x.ArticleName)
            .Map("count", x => x.TotalCount)
            .Map("producerName", x => x.Producer.Name)
            .Map("indicator", x => x.Indicator);

        new ArticleFullDto().Map("count", x => x.CurrentStock);

        new Producer().MapDefault(x => x.Id)
            .Map("id", x => x.Id)
            .Map("name", x => x.Name);
        new Purchase().MapDefault(x => x.PurchaseDatetime)
            .Map("dateTime", x => x.PurchaseDatetime)
            .Map("totalSum", x => x.Transaction.TransactionSum)
            .Map("id", x => x.Id);

        new Sale().MapDefault(x => x.SaleDatetime)
            .Map("dateTime", x => x.SaleDatetime)
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