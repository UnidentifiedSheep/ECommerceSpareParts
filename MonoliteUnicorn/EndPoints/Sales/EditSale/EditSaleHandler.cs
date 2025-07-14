using Core.Interface;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.CacheService;
using MonoliteUnicorn.Services.Sale;

namespace MonoliteUnicorn.EndPoints.Sales.EditSale;

public record EditSaleCommand(IEnumerable<EditSaleContentDto> EditedContent, string SaleId, int CurrencyId, 
    string UpdatedUserId, DateTime SaleDateTime, string? Comment, bool SellFromOtherStorages) : ICommand;

public class EditSaleValidation : AbstractValidator<EditSaleCommand>
{
    public EditSaleValidation()
    {
        RuleFor(x => x.Comment)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Trim().Length <= 256)
            .WithMessage("Максимальная длина общего комментария — 256 символов.");

        RuleFor(x => x.EditedContent)
            .NotEmpty()
            .WithMessage("Список содержимого продажи не может быть пустым.");

        RuleFor(x => x.SaleDateTime)
            .Must(date => date.Date >= DateTime.Now.Date.AddMonths(-3))
            .WithMessage("Дата продажи не может быть более чем трёхмесячной давности.");

        RuleFor(x => x.SaleDateTime)
            .Must(date => date.Date <= DateTime.Now.Date)
            .WithMessage("Дата продажи не может быть в будущем.");

        RuleForEach(x => x.EditedContent).ChildRules(content =>
        {
            content.RuleFor(x => x.Count)
                .GreaterThan(0)
                .WithMessage("Количество должно быть больше 0.");

            content.RuleFor(x => x.Comment)
                .Must(x => string.IsNullOrWhiteSpace(x) || x.Trim().Length <= 256)
                .WithMessage("Максимальная длина комментария у позиции — 256 символов.");

            content.RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Цена должна быть больше 0.");

            content.RuleFor(x => x.PriceWithDiscount)
                .GreaterThan(0)
                .WithMessage("Цена со скидкой должна быть больше 0.");

            content.RuleFor(x => x)
                .Must(x => x.PriceWithDiscount <= x.Price)
                .WithMessage("Цена со скидкой не может быть больше оригинальной цены.");
        });
    }
}

public class EditSaleHandler(DContext context, ISaleOrchestrator orchestrator, CacheQueue cacheQueue) : ICommandHandler<EditSaleCommand>
{
    public async Task<Unit> Handle(EditSaleCommand request, CancellationToken cancellationToken)
    {
        var articleIds = await context.SaleContents.AsNoTracking()
            .Select(x => x.ArticleId)
            .ToHashSetAsync(cancellationToken);
        await orchestrator.EditSale(request.EditedContent, request.SaleId, 
            request.CurrencyId, request.UpdatedUserId, 
            request.SaleDateTime, request.Comment, 
            request.SellFromOtherStorages, cancellationToken);
        articleIds.UnionWith(request.EditedContent.Select(x => x.ArticleId));
        
        cacheQueue.Enqueue(async sp =>
        {
            var cache = sp.GetRequiredService<IArticleCache>();
            await cache.ReCacheArticleModelsAsync(articleIds);
        });
        return Unit.Value;
    }
}