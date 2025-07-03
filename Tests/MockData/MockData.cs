using System.Drawing;
using System.Numerics;
using Bogus;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.Dtos.Amw.Producers;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.PostGres.Main;

namespace Tests.MockData;

public static class MockData
{
    public const string Locale = "ru";
    
    public static readonly List<string> Colors = Enum.GetNames(typeof(KnownColor)).ToList();

    public static List<NewArticleDto> CreateNewArticleDto(int count)
    {
        var f = new Faker<NewArticleDto>(Locale)
            .RuleFor(x => x.ArticleNumber, f => f.Lorem.Letter(28))
            .RuleFor(x => x.Name, f => string.Join(" ", f.Lorem.Words(4)))
            .RuleFor(x => x.Description, f => Random.Shared.Next(1, 2) == 1 ? string.Join(" ", f.Lorem.Words(9)) : null)
            .RuleFor(x => x.ProducerId, f => f.PickRandom(f.Random.Int(1, 5)))
            .RuleFor(x => x.Indicator, f => Random.Shared.Next(1, 2) == 1 ? f.PickRandom(Colors) : null)
            .RuleFor(x => x.IsOe, f => f.Random.Bool())
            .RuleFor(x => x.PackingUnit, f => f.PickRandom(3));
        return f.Generate(count);
    }

    public static List<AmwNewProducerDto> CreateNewProducerDto(int count)
    {
        var f = new Faker<AmwNewProducerDto>(Locale)
            .RuleFor(x => x.ProducerName, f => f.Lorem.Letter(28))
            .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
            .RuleFor(x => x.IsOe, f => f.Random.Bool());
        return f.Generate(count);
    }
    public static List<AspNetUser> CreateNewUser(int count)
    {
        var f = new Faker<AspNetUser>(Locale)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.NormalizedEmail, f => f.Person.Email.ToUpperInvariant())
            .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
            .RuleFor(x => x.Name, f => f.Person.FirstName)
            .RuleFor(x => x.Surname, f => f.Person.LastName)
            .RuleFor(x => x.UserName, f => f.Person.UserName)
            .RuleFor(x => x.Id, f => f.Person.UserName)
            .RuleFor(x => x.NormalizedUserName, f => f.Person.UserName.ToUpperInvariant())
            .RuleFor(x => x.PhoneNumberConfirmed, f => f.Random.Bool())
            .RuleFor(x => x.EmailConfirmed, f => f.Random.Bool())
            .RuleFor(x => x.LockoutEnabled, f => f.Random.Bool())
            .RuleFor(x => x.TwoFactorEnabled, f => f.Random.Bool());
        return f.Generate(count);
    }

    public static List<Storage> CreateNewStorage(int count)
    {
        var f = new Faker<Storage>(Locale)
            .RuleFor(x => x.Location, f => Random.Shared.Next(1, 2) == 1 ? f.Address.FullAddress() : null)
            .RuleFor(x => x.Description, f => Random.Shared.Next(1, 2) == 1 ? f.Commerce.ProductDescription() : null)
            .RuleFor(x => x.Name, f => f.Company.CompanyName());
        return f.Generate(count);
    }

    public static List<StorageContent> CreateStorageContent(IEnumerable<int> availableArticlesIds,
        IEnumerable<string> availableStorages, IEnumerable<int> availableCurrencyIds, int count)
    {
        var articleIds = availableArticlesIds.Distinct().ToList();
        var storages = availableStorages.Distinct().ToList();
        var currencyIds = availableCurrencyIds.Distinct().ToList();
        var f = new Faker<StorageContent>(Locale)
            .RuleFor(x => x.ArticleId, f => f.PickRandom(articleIds))
            .RuleFor(x => x.StorageName, f => f.PickRandom(storages))
            .RuleFor(x => x.BuyPrice, f => Math.Round(f.Random.Decimal(0.01m, 2000000), 2))
            .RuleFor(x => x.Count, f => f.Random.Int(1,1200))
            .RuleFor(x => x.CurrencyId, f => f.PickRandom(currencyIds));
        return f.Generate(count);
    }
    
    public static List<NewStorageContentDto> CreateNewStorageContentDto(IEnumerable<int> availableArticlesIds, 
        IEnumerable<int> availableCurrencyIds, int count)
    {
        var articleIds = availableArticlesIds.Distinct().ToList();
        var currencyIds = availableCurrencyIds.Distinct().ToList();
        var f = new Faker<NewStorageContentDto>(Locale)
            .RuleFor(x => x.ArticleId, f => f.PickRandom(articleIds))
            .RuleFor(x => x.BuyPrice, f => f.Random.Decimal(0.01m, 2000000))
            .RuleFor(x => x.Count, f => f.Random.Int(1,1200))
            .RuleFor(x => x.CurrencyId, f => f.PickRandom(currencyIds));
        return f.Generate(count);
    }

    public static List<SaleContentDetail> CreateSaleContentDetails(IEnumerable<int> availableStorageContentIds,
        IEnumerable<string> availableStorages, IEnumerable<int> availableCurrencyIds, int count)
    {
        var storageContentIds = availableStorageContentIds.Distinct().ToList();
        var storages = availableStorages.Distinct().ToList();
        var currencyIds = availableCurrencyIds.Distinct().ToList();
        
        var f = new Faker<SaleContentDetail>(Locale)
            .RuleFor(x => x.StorageContentId, f => f.Random.Int(1, 100) <= 50 ? f.PickRandom(storageContentIds) : null)
            .RuleFor(x => x.BuyPrice, f => f.Random.Decimal(0.01m, 2_000_000))
            .RuleFor(x => x.Count, f => f.Random.Int(1, 1200))
            .RuleFor(x => x.CurrencyId, f => f.PickRandom(currencyIds))
            .RuleFor(x => x.Storage, f => f.PickRandom(storages))
            .RuleFor(x => x.PurchaseDatetime,
                f => f.Date.Between(DateTime.Now.AddYears(-2), DateTime.Now.AddMonths(2)));
        return f.Generate(count);
    }

    public static List<Currency> CreateCurrency(int count)
    {
        var f = new Faker<Currency>(Locale)
            .RuleFor(x => x.Name, f => f.Person.UserName)
            .RuleFor(x => x.ShortName, f => f.Lorem.Letter(3))
            .RuleFor(x => x.CurrencySign, f => f.Lorem.Letter())
            .RuleFor(x => x.Code, f => f.Lorem.Letter(3));
        return f.Generate(count);
    }

    public static List<Transaction> CreateTransaction(IEnumerable<string> receiverIds, IEnumerable<string> senderIds, string whoMade, IEnumerable<int> currencyIds, int count)
    {
        var balanceCounter = new Dictionary<string, decimal>();
        var f = new Faker<Transaction>(Locale)
            .RuleFor(x => x.TransactionSum, f => Math.Round(f.Random.Decimal(1, 100000), 2))
            .RuleFor(x => x.SenderId, f => f.PickRandom(senderIds))
            .RuleFor(x => x.ReceiverId, (f, t) => {
                string receiver;
                do {
                    receiver = f.PickRandom(receiverIds);
                } while (receiver == t.SenderId);
                return receiver;
            })
            .RuleFor(x => x.TransactionDatetime,
                f => f.Date.Between(DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2)))
            .RuleFor(x => x.CurrencyId, f => f.PickRandom(currencyIds))
            .RuleFor(x => x.Status, _ => nameof(TransactionStatus.Normal))
            .RuleFor(x => x.WhoMadeUserId, _ => whoMade);
        
        var tr = f.Generate(count);

        foreach (var item in tr.OrderBy(x => x.TransactionDatetime))
        {
            if(!balanceCounter.TryGetValue(item.SenderId + item.CurrencyId, out var senderBalance))
                senderBalance = 0;
            if(!balanceCounter.TryGetValue(item.ReceiverId + item.CurrencyId, out var receiverBalance))
                receiverBalance = 0;
            item.SenderBalanceAfterTransaction = senderBalance;
            item.ReceiverBalanceAfterTransaction = receiverBalance;
            item.SenderBalanceAfterTransaction -= item.TransactionSum;
            item.ReceiverBalanceAfterTransaction += item.TransactionSum;
            balanceCounter[item.SenderId + item.CurrencyId] = item.SenderBalanceAfterTransaction;
            balanceCounter[item.ReceiverId + item.CurrencyId] = item.ReceiverBalanceAfterTransaction;
        }
        return tr;
    }
}