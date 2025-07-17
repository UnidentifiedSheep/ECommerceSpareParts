using System.Text;
using Core.Interface;
using Core.StaticFunctions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Exceptions.Sales;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.ArticleReservations;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Sale;

namespace MonoliteUnicorn.EndPoints.Sales.CreateSale;

public record CreateSaleCommand(string CreatedUserId, string BuyerId, int CurrencyId, string StorageName, bool SellFromOtherStorages,
    DateTime SaleDateTime, IEnumerable<NewSaleContentDto> SaleContent, string? Comment, decimal? PayedSum, string? ConfirmationCode) : ICommand;

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

public class CreateSaleHandler(ISaleOrchestrator saleOrchestrator, DContext context, IArticleReservation reservationService, CacheQueue cacheQueue) : ICommandHandler<CreateSaleCommand, Unit>
{
    public async Task<Unit> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var dateTimeWithoutTimeZone = DateTime.SpecifyKind(request.SaleDateTime, DateTimeKind.Unspecified);
        var saleContentByArticle = request.SaleContent
            .GroupBy(x => x.ArticleId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));

        var (notEnoughByReservation, notEnoughByStock) = await reservationService
            .GetArticlesWithNotEnoughStock(request.BuyerId, request.StorageName,
            request.SellFromOtherStorages, saleContentByArticle, cancellationToken);

        if (notEnoughByStock.Count != 0)
            throw new NotEnoughCountOnStorageException(notEnoughByStock.Keys);
        if (notEnoughByReservation.Count != 0)
        {
            var arts = await context.Articles
                .AsNoTracking()
                .Where(x => notEnoughByReservation.Keys.Contains(x.Id))
                .Select(x => new { x.Id, ProducerName = x.Producer.Name, x.ArticleNumber })
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            var res = new Dictionary<string, int>();
            var codeBuilder = new StringBuilder();
            codeBuilder.Append(ConcurrencyStatic.GetConcurrencyCode(request.CreatedUserId));
            foreach (var (id, count) in notEnoughByReservation.OrderBy(x => x.Key))
            {
                var art = arts[id];
                var key = $"{art.ProducerName}_{art.ArticleNumber}";
                res[key] = count;
                codeBuilder.Append(ConcurrencyStatic.GetConcurrencyCode(key, count));
            }
            var currentCode = codeBuilder.ToString();
            if (currentCode != request.ConfirmationCode)
                throw new SoftConfirmationNeededException(currentCode, res);
        }
        
        
        await saleOrchestrator.CreateFullSale(request.CreatedUserId, request.BuyerId, request.CurrencyId, request.StorageName, 
            request.SellFromOtherStorages, dateTimeWithoutTimeZone, request.SaleContent, request.Comment, request.PayedSum, cancellationToken);
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(saleContentByArticle.Keys);
        });
        return Unit.Value;
    }
}