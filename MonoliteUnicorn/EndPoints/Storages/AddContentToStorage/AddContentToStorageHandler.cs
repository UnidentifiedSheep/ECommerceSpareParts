using Core.Interface;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Inventory;

namespace MonoliteUnicorn.EndPoints.Storages.AddContentToStorage;

public record AddContentToStorageCommand(IEnumerable<NewStorageContentDto> StorageContent, string StorageName, string UserId) : ICommand;

public class AddContentToStorageValidation : AbstractValidator<AddContentToStorageCommand>
{
    public AddContentToStorageValidation()
    {
        RuleForEach(x => x.StorageContent).ChildRules(content =>
        {
            content.RuleFor(x => x.BuyPrice)
                .Must(x => Math.Round(x, 2) > 0)
                .WithMessage("Цена не может быть меньше или равна 0");
            content.RuleFor(x => x.Count)
                .GreaterThan(0)
                .WithMessage("Количество должно быть больше 0");
        });
        RuleFor(x => x.StorageContent)
            .NotEmpty()
            .WithMessage("Список новых позиций не может быть пуст");
        RuleFor(x => x.StorageName)
            .NotEmpty()
            .WithMessage("Название склада не может быть пустым");
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Id пользователя не может быть пустым");
    }
}

public class AddContentToStorageHandler(IInventory inventoryService, CacheQueue cacheQueue) : ICommandHandler<AddContentToStorageCommand, Unit>
{
    public async Task<Unit> Handle(AddContentToStorageCommand request, CancellationToken cancellationToken)
    {
        var asTupleList = new List<(int, int, decimal, int)>();
        var articleIds = new HashSet<int>();
        foreach (var item in request.StorageContent)
        {
            asTupleList.Add((item.ArticleId, item.Count, item.BuyPrice, item.CurrencyId));
            articleIds.Add(item.ArticleId);
        }
        await inventoryService.AddContentToStorage(asTupleList, request.StorageName, 
            request.UserId, StorageMovementType.StorageContentAddition, cancellationToken);
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        return Unit.Value;
    }
}