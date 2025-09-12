using Application.Handlers.Articles.CreateArticles;
using Application.Handlers.Balance.CreateTransaction;
using Application.Handlers.Producers.CreateProducer;
using Application.Handlers.Sales.CreateSale;
using Application.Handlers.StorageContents.AddContent;
using Application.Handlers.Storages.CreateStorage;
using Application.Handlers.Users.CreateUser;
using Core.Dtos.Amw.Sales;
using Core.Entities;
using Core.Enums;
using Core.Models;
using Mapster;
using MediatR;
using Persistence.Contexts;
using static Tests.MockData.MockData;

namespace Tests.MockData;

public static class MockMediatr
{
    public static async Task AddMockProducersAndArticles(this IMediator mediator)
    {
        var newProducerModel = CreateNewProducerDto(10);
        var producerIds = new List<int>();
        foreach (var model in newProducerModel)
        {
            var producerCommand = new CreateProducerCommand(model);
            var result = await mediator.Send(producerCommand);
            producerIds.Add(result.ProducerId);
        }
        
        var articleList = CreateNewArticleDto(10);
        foreach (var article in articleList)
            article.ProducerId = Global.Faker.PickRandom(producerIds);
        
        var articleCommand = new CreateArticlesCommand(articleList);
        await mediator.Send(articleCommand);
    }

    public static async Task<string> AddMockUser(this IMediator mediator)
    {
        var user = GetUserDto();
        var command = new CreateUserCommand(user);
        
        var result = await mediator.Send(command);
        return result.UserId;
    }

    public static async Task<Transaction> AddMockTransaction(this IMediator mediator, string sender, string receiver, string whoCreated, 
        decimal amount = 100, DateTime? when = null)
    {
        var command = new CreateTransactionCommand(
            SenderId: sender,
            ReceiverId: receiver,
            Amount: amount,
            CurrencyId: 1,
            WhoCreatedTransaction: whoCreated,
            TransactionDateTime: when ?? DateTime.Now,
            TransactionStatus.Normal
        );
        var result = await mediator.Send(command);
        return result.Transaction;
    }
    
    public static async Task AddMockStorage(this IMediator mediator)
    {
        var storage = CreateNewStorage(1)[0];
        var command = new CreateStorageCommand(storage.Name, storage.Description, storage.Location);
        await mediator.Send(command);
    }

    public static async Task AddMockStorageContents(this IMediator mediator, IEnumerable<int> articleIds, int currencyId,
        string storageName, string userId, int count = 20)
    {
        var dtoList = CreateNewStorageContentDto(articleIds, [currencyId], count)
            .ToList();

        var command = new AddContentCommand(dtoList, storageName, userId, StorageMovementType.StorageContentAddition);
        await mediator.Send(command);
    }

    public static async Task AddMockSale(this IMediator mediator, IEnumerable<StorageContent> storageContents, int currencyId,
        string userId, string transactionId, string storageName, DateTime? when = null)
    {
        var saleContent = new List<NewSaleContentDto>();
        var storageContentValues = new List<PrevAndNewValue<StorageContent>>();
        var articlesTakenCount = new Dictionary<int, int>();
        
        foreach (var content in storageContents)
        {
            var newValue = content.Adapt<StorageContent>();
            newValue.Count = 0;
            saleContent.Add(new NewSaleContentDto
            {
                ArticleId = content.ArticleId,
                Count = content.Count,
                Comment = Global.Faker.Lorem.Letter(10),
                Price = content.BuyPrice,
                PriceWithDiscount = content.BuyPrice
            });
            articlesTakenCount[content.ArticleId] = articlesTakenCount.GetValueOrDefault(content.ArticleId) + content.Count;
            
            storageContentValues.Add(new PrevAndNewValue<StorageContent>(content.Adapt<StorageContent>(), newValue));
        }
        
        var command = new CreateSaleCommand(saleContent, storageContentValues, currencyId, userId, userId, 
            transactionId, storageName, when ?? DateTime.Now, null);
        await mediator.Send(command);
    }
}