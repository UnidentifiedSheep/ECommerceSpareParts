using Abstractions;
using Extensions;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Entities;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;

namespace Main.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        new Product().MapDefault(x => x.Id)
            .Map("id", x => x.Id)
            .Map("articleNumber", x => x.Sku)
            .Map("title", x => x.Name)
            .Map("count", x => x.Stock)
            .Map("producerName", x => x.Producer.Name)
            .Map("indicator", x => x.Indicator)
            .Map("popularity", x => x.Popularity);

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
            .MapDefault(x => x.CreatedAt)
            .Map("id", x => x.Id)
            .Map("createAt", x => x.CreatedAt)
            .Map("updatedAt", x => x.UpdatedAt)
            .Map("isDone", x => x.IsDone);
    }
}