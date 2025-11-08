using FluentValidation;

namespace Main.Application.Handlers.Balance.GetTransactions;

public class GetTransactionsValidation : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsValidation()
    {
        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => x.ReceiverId != x.SenderId || x.ReceiverId == null)
            .WithMessage("Отправитель и получатель не могут быть одинаковы");

        RuleFor(x => new { x.ReceiverId, x.SenderId })
            .Must(x => x.ReceiverId != null || x.SenderId != null)
            .WithMessage("Должен быть указан хотя бы один: отправитель или получатель");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeStart.Date <= x.RangeEnd.Date)
            .WithMessage("Дата начала диапазона не может быть позже даты конца");

        RuleFor(x => new { x.RangeStart, x.RangeEnd })
            .Must(x => x.RangeEnd.Date <= x.RangeStart.Date.AddMonths(5))
            .WithMessage("Максимальный диапазон выборки — 5 месяцев");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}