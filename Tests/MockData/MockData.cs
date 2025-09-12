using System.Drawing;
using Bogus;
using Core.Dtos.Amw.Producers;
using Core.Dtos.Amw.Storage;
using Core.Dtos.Amw.Users;
using Core.Dtos.Services.Articles;
using Core.Entities;
using Core.Enums;

namespace Tests.MockData;

public static class MockData
{
    public const string Locale = "ru";
    
    public static readonly List<string> Colors = Enum.GetNames(typeof(KnownColor)).ToList();

    public static List<CreateArticleDto> CreateNewArticleDto(int count)
    {
        var f = new Faker<CreateArticleDto>(Locale)
            .RuleFor(x => x.ArticleNumber, f => f.Lorem.Letter(28))
            .RuleFor(x => x.Name, f => string.Join(" ", f.Lorem.Words(4)))
            .RuleFor(x => x.Description, f => Random.Shared.Next(1, 2) == 1 ? string.Join(" ", f.Lorem.Words(9)) : null)
            .RuleFor(x => x.ProducerId, f => f.PickRandom(f.Random.Int(1, 5)))
            .RuleFor(x => x.Indicator, f => Random.Shared.Next(1, 2) == 1 ? f.PickRandom(Colors) : null)
            .RuleFor(x => x.IsOe, f => f.Random.Bool())
            .RuleFor(x => x.PackingUnit, f => f.PickRandom(3));
        return f.Generate(count);
    }

    public static List<NewProducerDto> CreateNewProducerDto(int count)
    {
        var f = new Faker<NewProducerDto>(Locale)
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
    
    public static NewUserDto GetUserDto()
    {
        var f = new Faker<NewUserDto>(Locale)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.UserName, f => f.Person.UserName)
            .RuleFor(x => x.PhoneNumber, f => "+7" + f.Person.Phone)
            .RuleFor(x => x.Name, f => f.Person.FirstName)
            .RuleFor(x => x.Surname, f => f.Person.LastName)
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.Roles, ["Member"]);
        return f.Generate();
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
            .RuleFor(x => x.BuyPrice, f => Math.Round((f.Random.Decimal() * 100000) + 1, 2))
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
            .RuleFor(x => x.BuyPrice, f => Math.Round((f.Random.Decimal() * 100000) + 1, 2))
            .RuleFor(x => x.Count, f => f.Random.Int(1,1200))
            .RuleFor(x => x.PurchaseDate, DateTime.Now)
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
            .RuleFor(x => x.BuyPrice, f => Math.Round((f.Random.Decimal() * 100000) + 1, 2))
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
        var r = receiverIds.ToList();
        var balanceCounter = new Dictionary<string, decimal>();
        var f = new Faker<Transaction>(Locale)
            .RuleFor(x => x.TransactionSum, f => Math.Round(f.Random.Decimal(1, 100000), 2))
            .RuleFor(x => x.SenderId, f => f.PickRandom(senderIds))
            .RuleFor(x => x.ReceiverId, (f, t) => {
                string receiver;
                do {
                    receiver = f.PickRandom(r);
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

    public static Sale CreateSale(string transactionId, string buyerId, string whoMade, string storageName,
        int currencyId)
    {
        var f = new Faker<Sale>(Locale)
            .RuleFor(x => x.Comment, f => f.Random.Int(1, 100) < 50 ? f.Lorem.Letter(100) : null)
            .RuleFor(x => x.CreatedUserId, whoMade)
            .RuleFor(x => x.BuyerId, buyerId)
            .RuleFor(x => x.TransactionId, transactionId)
            .RuleFor(x => x.CurrencyId, currencyId)
            .RuleFor(x => x.MainStorageName, storageName)
            .RuleFor(x => x.SaleDatetime, f => f.Date.Between(DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2)));
        return f.Generate(1).Single();
    }

    public static List<SaleContent> CreateSaleContent(IEnumerable<int> articleIds, int count)
    {
        var f = new Faker<SaleContent>(Locale)
            .RuleFor(x => x.ArticleId, f => f.PickRandom(articleIds))
            .RuleFor(x => x.Count, f => f.Random.Int(1, 1200))
            .RuleFor(x => x.Price, f => f.Random.Decimal(0.01m, 2_000_000))
            .RuleFor(x => x.Discount, f => f.Random.Decimal(0.01m, 100));
        return f.Generate(count);
    }

    public static void CreateSaleContentDetail(IEnumerable<SaleContent> saleContents, IEnumerable<int> currencyIds, IEnumerable<string> storageNames)
    {
        var faker = new Faker(Locale);
        var crs = currencyIds.ToList();
        var strgs = storageNames.ToList();
        foreach (var content in saleContents)
        {
            content.SaleContentDetails.Add(new SaleContentDetail
            {
                BuyPrice = faker.Random.Decimal(1, 100),
                Count = content.Count,
                CurrencyId = faker.PickRandom(crs),
                Storage = faker.PickRandom(strgs),
                PurchaseDatetime = faker.Date.Between(DateTime.Now.AddMonths(-2), DateTime.Now.AddMonths(2))
            });
        }
    }
}