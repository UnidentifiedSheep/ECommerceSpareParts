using Core.Interface;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Sale;

namespace MonoliteUnicorn.EndPoints.Sales.CreateSale;

public record CreateSaleCommand(string CreatedUserId, string BuyerId, int CurrencyId, string StorageName, bool SellFromOtherStorages,
    DateTime SaleDateTime, IEnumerable<NewSaleContentDto> SaleContent, string? Comment, decimal? PayedSum) : ICommand<Unit>;

public class CreateSaleValidation : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidation()
    {
        RuleFor(x => x.SaleContent).NotEmpty()
            .WithMessage("Продажа не может быть пустой");
        RuleFor(x => x.SaleContent)
            .Must(x => !x.Any(z => z.Count <= 0 || z.Price <= 0 || z.PriceWithDiscount <= 0))
            .WithMessage("Цена или количество у позиции не может быть равно 0");
        RuleFor(x => x.BuyerId).NotEmpty()
            .WithMessage("Id покупателя не может быть пустым");
        RuleFor(x => x.CreatedUserId).NotEmpty()
            .WithMessage("Id пользователя создавшего закупку не может быть пустым");
        RuleFor(x => x.SaleDateTime).Must(x => x > DateTime.Now.AddMonths(-3))
            .WithMessage("Дата не может быть раньше чем за 3 месяца от сегодняшнего дня");
    }
}

public class CreateSaleHandler(ISaleOrchestrator saleOrchestrator, CacheQueue cacheQueue) : ICommandHandler<CreateSaleCommand, Unit>
{
    public async Task<Unit> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var dateTimeWithoutTimeZone = DateTime.SpecifyKind(request.SaleDateTime, DateTimeKind.Unspecified);
        var articleIds = request.SaleContent.Select(x => x.ArticleId).ToHashSet();
        await saleOrchestrator.CreateFullSale(request.CreatedUserId, request.BuyerId, request.CurrencyId, request.StorageName, 
            request.SellFromOtherStorages, dateTimeWithoutTimeZone, request.SaleContent, request.Comment, request.PayedSum, cancellationToken);
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        return Unit.Value;
    }
}