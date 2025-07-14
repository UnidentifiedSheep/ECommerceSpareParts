using Core.Interface;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Purchase;

namespace MonoliteUnicorn.EndPoints.Purchase.CreatePurchase;

public record CreatePurchaseCommand(string CreatedUserId, string SupplierId, int CurrencyId, string StorageName, DateTime PurchaseDate, 
    IEnumerable<NewPurchaseContentDto> PurchaseContent, string? Comment, decimal? PayedSum) : ICommand<Unit>;

public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x => x.PurchaseContent).NotEmpty()
            .WithMessage("Закупка не может быть пустой");
        RuleFor(x => x.PurchaseContent)
            .Must(x => !x.Any(z => z.Count <= 0 || z.Price <= 0))
            .WithMessage("Цена или количество у позиции не может быть равно 0");
        RuleFor(x => x.SupplierId).NotEmpty()
            .WithMessage("Id продавца не может быть пустым");
        RuleFor(x => x.CreatedUserId).NotEmpty()
            .WithMessage("Id пользователя создавшего закупку не может быть пустым");
        RuleFor(x => x.PurchaseDate).Must(x => x > DateTime.Now.AddMonths(-3))
            .WithMessage("Дата не может быть раньше чем за 3 месяца от сегодняшнего дня");
        
    }
}

public class CreatePurchaseHandler(IPurchaseOrchestrator purchaseOrchestrator, CacheQueue cacheQueue) : ICommandHandler<CreatePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var dateTimeWithoutTimeZone = DateTime.SpecifyKind(request.PurchaseDate, DateTimeKind.Unspecified);
        await purchaseOrchestrator.CreateFullPurchase(request.CreatedUserId, request.SupplierId, 
            request.CurrencyId, request.StorageName, dateTimeWithoutTimeZone, request.PurchaseContent, request.Comment, request.PayedSum, cancellationToken);
        var articleIds = request.PurchaseContent
            .Select(x => x.ArticleId)
            .ToHashSet();
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        return Unit.Value;
    }
}