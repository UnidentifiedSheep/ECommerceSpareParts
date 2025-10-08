using FluentValidation;

namespace Main.Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;

public class GetArticlesWithNotEnoughStockValidation : AbstractValidator<GetArticlesWithNotEnoughStockQuery>
{
    public GetArticlesWithNotEnoughStockValidation()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty()
            .WithMessage("Id пользователя, чьи резервации будут браться в расчет не может быть пустым");
        RuleFor(x => x.StorageName)
            .NotEmpty()
            .WithMessage("Название склада не может быть пустым");
        RuleForEach(x => x.NeededCounts)
            .ChildRules(z =>
            {
                z.RuleFor(x => x.Value)
                    .GreaterThan(0)
                    .WithMessage("Запрашиваемое количество артикула должно быть больше 0");
            });
    }
}